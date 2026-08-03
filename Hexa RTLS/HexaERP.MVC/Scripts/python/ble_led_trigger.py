import asyncio
import secrets
from bleak import BleakClient
from Crypto.Cipher import AES
import sys

# ==========================
# CONFIGURATION
# ==========================


if len(sys.argv) > 1:
    DEVICE_ADDRESS = sys.argv[1]
else:
    print("BLE MAC Address not provided.")
    sys.exit(1)
WRITE_UUID     = "0000FF02-0000-1000-8000-00805F9B34FB"
NOTIFY_UUID    = "0000FF01-0000-1000-8000-00805F9B34FB"

DEVICE_KEY = bytes([
    0x12, 0x34, 0x56, 0x78,
    0x9A, 0xBC, 0xDE, 0xF0,
    0x11, 0x22, 0x33, 0x44,
    0x55, 0x66, 0x77, 0x88,
])

# ==========================
# Global State
# ==========================

device_random:   bytes | None  = None
login_success:   bool          = False
challenge_event: asyncio.Event = None
login_event:     asyncio.Event = None

# ==========================
# Utility
# ==========================

def checksum(data: bytes | bytearray) -> int:
    return sum(data) & 0xFF


def aes_ecb_encrypt(key: bytes, plaintext: bytes) -> bytes:
    return AES.new(key, AES.MODE_ECB).encrypt(plaintext)


# ==========================
# Packet Builders
# ==========================

def build_cmd06(mobile_random: bytes) -> bytearray:
    """
    CMD 0x06 — Auth challenge
    Format: 06 | 11 | [8 bytes random_m] | [9 bytes 0x00] | checksum
    """
    packet = bytearray()
    packet.append(0x06)
    packet.append(0x11)              # LEN = 17
    packet.extend(mobile_random)     # 8 bytes
    packet.extend(b'\x00' * 9)      # 9 padding bytes
    packet.append(checksum(packet))
    return packet


def build_cmd07(auth_token: bytes) -> bytearray:
    """
    CMD 0x07 — Auth response
    Format: 07 | 11 | [16 bytes AES token] | [1 byte 0x00] | checksum
    """
    packet = bytearray()
    packet.append(0x07)
    packet.append(0x11)              # LEN = 17
    packet.extend(auth_token)        # 16 bytes
    packet.extend(b'\x00' * 1)      # 1 padding byte
    packet.append(checksum(packet))
    return packet


def build_cmd(cmd: int, payload: bytes | bytearray) -> bytearray:
    """Generic packet builder for LED / Buzzer etc."""
    packet = bytearray()
    packet.append(cmd)
    packet.append(len(payload))
    packet.extend(payload)
    packet.append(checksum(packet))
    return packet


# ==========================
# Notification Handler
# ==========================

def on_notify(sender, data: bytearray) -> None:
    global device_random, login_success

    print(f"NOTIFY  : {data.hex()}")
    print(f"  CMD=0x{data[0]:02X}  len={len(data)}")

    if not data:
        return

    # CMD 0x86 — device random challenge
    # Format: 86 | 11 | [8 bytes random] | [9 bytes 0x00] | checksum
    if data[0] == 0x86 and len(data) >= 10:
        device_random = bytes(data[2:10])
        print(f"  Device Random : {device_random.hex()}")
        challenge_event.set()

    # CMD 0x87 — login result
    # Format: 87 | 11 | [status] | [padding] | checksum
    elif data[0] == 0x87:
        status = data[2] if len(data) >= 3 else 0xFF   # ← fix: data[2] not data[1]
        print(f"  Status byte   : 0x{status:02X}")
        if status == 0x00:
            login_success = True
            print("  Authentication : SUCCESS ✅")
        else:
            print(f"  Authentication : FAILED ❌ (status=0x{status:02X})")
        login_event.set()


# ==========================
# Main
# ==========================

async def main() -> None:
    global device_random, login_success
    global challenge_event, login_event

    # Reset state
    device_random   = None
    login_success   = False
    challenge_event = asyncio.Event()
    login_event     = asyncio.Event()

    async with BleakClient(DEVICE_ADDRESS, timeout=20.0) as client:
        print(f"Connected : {client.is_connected}")

        await client.start_notify(NOTIFY_UUID, on_notify)
        await asyncio.sleep(0.5)

        # ---------------------
        # Step 1 — CMD 0x06
        # ---------------------

        mobile_random = secrets.token_bytes(8)
        print(f"\nMobile Random : {mobile_random.hex()}")

        packet = build_cmd06(mobile_random)
        print(f"SEND CMD 06   : {packet.hex()}")
        print(f"  breakdown   : CMD=06 LEN=11 random={mobile_random.hex()} padding=00*9")

        await client.write_gatt_char(WRITE_UUID, packet, response=False)

        try:
            await asyncio.wait_for(challenge_event.wait(), timeout=10.0)
        except asyncio.TimeoutError:
            print("No challenge from device. Aborting.")
            await client.stop_notify(NOTIFY_UUID)
            return

        # ---------------------
        # Step 2 — CMD 0x07
        # ---------------------

        auth_token = aes_ecb_encrypt(DEVICE_KEY, mobile_random + device_random)
        print(f"\nAuth Token    : {auth_token.hex()}")

        packet = build_cmd07(auth_token)
        print(f"SEND CMD 07   : {packet.hex()}")
        print(f"  breakdown   : CMD=07 LEN=11 token={auth_token.hex()} padding=00*1")

        await client.write_gatt_char(WRITE_UUID, packet, response=False)

        try:
            await asyncio.wait_for(login_event.wait(), timeout=10.0)
        except asyncio.TimeoutError:
            print("Login timeout. Aborting.")
            await client.stop_notify(NOTIFY_UUID)
            return

        if not login_success:
            print("Login rejected. Aborting.")
            await client.stop_notify(NOTIFY_UUID)
            return

        # ---------------------
        # Red LED — CMD 0x14
        # ---------------------

        packet = build_cmd(0x14, bytearray([0x10, 10]))
        print(f"\nSEND LED      : {packet.hex()}")
        await client.write_gatt_char(WRITE_UUID, packet, response=False)
        await asyncio.sleep(12)

        # ---------------------
        # Buzzer — CMD 0x15
        # ---------------------

        packet = build_cmd(0x15, bytearray([5, 0]))
        print(f"SEND BUZZER   : {packet.hex()}")
        await client.write_gatt_char(WRITE_UUID, packet, response=False)
        await asyncio.sleep(5)

        await client.stop_notify(NOTIFY_UUID)
        print("\nDone. ✅")


if __name__ == "__main__":
    asyncio.run(main())