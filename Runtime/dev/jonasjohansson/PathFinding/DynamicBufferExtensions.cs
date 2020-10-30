using Unity.Entities;

namespace dev.jonasjohansson.PathFinding
{
    public static class DynamicBufferExtensions {
        public static void Reverse<T>(this DynamicBuffer<T> buffer)
            where T : struct
        {
            var length = buffer.Length;
            var i = 0;

            for (var j = length - 1; i < j; --j)
            {
                var obj = buffer[i];
                buffer[i] = buffer[j];
                buffer[j] = obj;
                ++i;
            }
        }
    }
}