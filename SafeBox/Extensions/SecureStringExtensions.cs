using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace SafeBox.Extensions;

public static class SecureStringExtensions
{
    public static bool IsNull(this SecureString secureString) => secureString == null || secureString.Length == 0;

    /// <summary>
    /// Fill a block of memory with zeros, given a pointer to the block and the length, in bytes, to be filled.
    /// </summary>
    /// <param name="destination">A pointer to the memory block to be filled with zeros.</param>
    /// <param name="length">The number of bytes to fill with zeros.</param>
    [DllImport("kernel32.dll")]
    private static extern void RtlZeroMemory(IntPtr destination, int length);

    /// <summary>
    /// Hash a SecureString given a hashing algorithm.
    /// Encodes the SecureString with the given encoding before hashing.
    /// </summary>
    /// <param name="secureString">The SecureString to hash.</param>
    /// <param name="hashAlgorithm">The hashing algorithm to hash the SecureString with.</param>
    /// <exception cref="ArgumentNullException"><paramref name="secureString"/>, <paramref name="hashAlgorithm"/> or <paramref name="encoding"/> is null.</exception>
    /// <exception cref="NotSupportedException">The current computer is not running Windows 2000 Service Pack 3 or later.</exception>
    /// <exception cref="OutOfMemoryException">There is insufficient memory available.</exception>
    /// <exception cref="EncoderFallbackException">An encoding fallback occurred (see Character Encoding in the .NET Framework for complete explanation) and <see cref="System.Text.Encoding.EncoderFallback"/> is set to <see cref="System.Text.EncoderExceptionFallback"/> in <paramref name="encoding"/>.</exception>
    /// <returns>An array of hashed bytes. If <paramref name="secureString"/> is empty, returns an empty array.</returns>
    public unsafe static byte[] GetHashBytes(this SecureString secureString, HashAlgorithm hashAlgorithm)
    {
        if (secureString == null || hashAlgorithm == null)
            return [];

        // An empty string is always zero bytes, but can still be hashed
        if (secureString.Length == 0)
            return hashAlgorithm.ComputeHash([]);

        var bstr = IntPtr.Zero;
        var encodedBuffer = IntPtr.Zero;
        var encodedBytesCount = 0;
        byte[] encodedBytes = null;
        GCHandle encodedBytesPin = default;

        // 1. Marshal the secure string to unmanaged memory.
        // 2. Create a buffer within memory and encode the secure string into
        //    the buffer using the given encoding.
        // 3. Create a managed array and pin it with the GC.
        // 4. Copy the buffer into the managed array.
        // 5. Return the hash of the managed array.
        try
        {
            // Marshal the secure string to memory
            bstr = Marshal.SecureStringToBSTR(secureString);

            // Calculate the maximum number of bytes that may be needed to
            // represent the same string in the specified encoding instead
            // of UTF-16.
            int maxEncodedBytesCount = Encoding.UTF8.GetMaxByteCount(secureString.Length);

            // Allocate memory large enough to store the maximum number
            // of encoded bytes.
            encodedBuffer = Marshal.AllocHGlobal(maxEncodedBytesCount);

            // Get pointers to the UTF-16 string and encoded buffer
            var utf16CharsPtr = (char*)bstr.ToPointer();
            var encodedBytesPtr = (byte*)encodedBuffer.ToPointer();

            // Read the UTF-16 string, encode it into the specified encoding
            // and store it in the encoded buffer, getting the number of
            // bytes stored.
            encodedBytesCount = Encoding.UTF8.GetBytes(utf16CharsPtr, secureString.Length, encodedBytesPtr, maxEncodedBytesCount);

            // Zero out and free the secure string memory
            Marshal.ZeroFreeBSTR(bstr);
            bstr = IntPtr.Zero;

            // Create a managed array to store the bytes that are now encoded
            encodedBytes = new byte[encodedBytesCount];

            // Pin the array with the GC to prevent it being moved
            encodedBytesPin = GCHandle.Alloc(encodedBytes, GCHandleType.Pinned);

            // Copy the unmanaged buffer into the byte array
            Marshal.Copy(encodedBuffer, encodedBytes, 0, encodedBytesCount);

            // Zero out and free the buffer
            RtlZeroMemory(encodedBuffer, encodedBytesCount);
            Marshal.FreeHGlobal(encodedBuffer);
            encodedBuffer = IntPtr.Zero;

            // Compute and return the hash of the managed bytes
            return hashAlgorithm.ComputeHash(encodedBytes);
        }
        finally
        {
            if (bstr != IntPtr.Zero)
            {
                // The secure string memory was not zeroed out and freed, do
                // this now.
                Marshal.ZeroFreeBSTR(bstr);
            }

            if (encodedBuffer != IntPtr.Zero)
            {
                // The buffer was not zeroed out and freed, do this now.
                RtlZeroMemory(encodedBuffer, encodedBytesCount);
                Marshal.FreeHGlobal(encodedBuffer);
            }

            if (encodedBytes != null)
            {
                // Zero out the managed bytes
                for (int i = 0; i < encodedBytes.Length; i++)
                    encodedBytes[i] = 0;

                // Release the GC pin on the managed bytes
                encodedBytesPin.Free();
            }
        }
    }
}