using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiddleFiddle
{

    /// <summary>
    /// Provide Hex-Related Logic
    /// </summary>
    public static class FiddleFiddleHex
    {
        /// <summary>
        /// It provides hexlified string with specified length.
        /// It is similar with python's `binascii.hexlify()` function
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static string ConvertByteArrayToHexlifiedString(byte[] bytes, int limit)
        {
            // 참고. http://www.codegist.net/code/converting-hex-string-to-byte-array-c%23/

            // 출력할 byte[] 의 원소의 갯수를 제한
            int max = 10;
            if (bytes.Count() < limit)
                max = bytes.Count();
            else // bytes.Count() >= limit
                max = limit;

            StringBuilder sb = new StringBuilder();

            for(int i=0; i < max; i++)
            {
                sb.Append(string.Format("0x{0:X} ", bytes[i]));
            }

            return sb.ToString();
        }

        /// <summary>
        /// It is more efficient than ConvertByteArrayToHexlifiedString(byte[], int)
        /// It uses Array[].Take(int) method to slice long array.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static string ConvertByteArrayToHexlifiedStringEx(byte[] bytes, int limit)
        {
            // 참고. http://www.codegist.net/code/converting-hex-string-to-byte-array-c%23/
            // 개선 참고. https://stackoverflow.com/questions/406485/array-slices-in-c-sharp

            // 출력할 byte[] 의 원소의 갯수를 제한
            var sliced = bytes.Take(limit);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < sliced.Count(); i++)
            {
                // Instead Of sliced[i], I use ElementAt method. 
                // This is because IEnumerable doesn't allow me to use "[] indexing"
                sb.Append(string.Format("0x{0:X} ", sliced.ElementAt(i)));
            }

            return sb.ToString();
        }

        /// <summary>
        /// CAUTION!! it takes a much time than how long time you think it takes.
        /// I recommend you to use the alternatives.
        ///     * ConvertByteArrayToHexlifiedString(byte[] bytes, int limit)
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ConvertByteArrayToHexlifiedString(byte[] bytes)
        {
            // 참고. http://www.codegist.net/code/converting-hex-string-to-byte-array-c%23/

            // 출력할 byte[] 의 원소의 갯수를 제한
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < bytes.Count(); i++)
            {
                sb.Append(string.Format("0x{0:X} ", bytes[i]));
            }

            return sb.ToString();
        }
    }
}
