using System;
using System.Collections.Generic;
using System.IO;
/// <summary>
/// TcpManager에서 사용하는 소켓의 추가 버퍼.
/// 실제 Send와 Receive를 하지 않고 버퍼에 저장하여 실시간성을 확보.
/// </summary>
public class PacketQueue
{
    struct PacketInfo
    {
        public int offset;
        public int size;
    };

    private MemoryStream m_streamBuffer;

    private List<PacketInfo> m_offsetList;

    private int m_offset = 0;


    private Object lockObj = new Object();

    public PacketQueue()
    {
        m_streamBuffer = new MemoryStream();
        m_offsetList = new List<PacketInfo>();
    }

    public int Enqueue(byte[] data, int size)
    {
        PacketInfo info = new PacketInfo();

        info.offset = m_offset;
        info.size = size;

        lock (lockObj)
        {
            m_offsetList.Add(info);
            m_streamBuffer.Position = m_offset;
            m_streamBuffer.Write(data, 0, size);
            m_streamBuffer.Flush();
            m_offset += size;
        }

        return size;
    }

    public int Dequeue(ref byte[] buffer, int size)
    {
        if (m_offsetList.Count <= 0)
        {
            return -1;
        }

        int recvSize = 0;
        lock (lockObj)
        {
            PacketInfo info = m_offsetList[0];
            int dataSize = Math.Min(size, info.size);
            m_streamBuffer.Position = info.offset;
            recvSize = m_streamBuffer.Read(buffer, 0, dataSize);
            if (recvSize > 0)
            {
                m_offsetList.RemoveAt(0);
            }
            if (m_offsetList.Count == 0)
            {
                Clear();
                m_offset = 0;
            }
        }

        return recvSize;
    }
    public void Clear()
    {
        byte[] buffer = m_streamBuffer.GetBuffer();
        Array.Clear(buffer, 0, buffer.Length);

        m_streamBuffer.Position = 0;
        m_streamBuffer.SetLength(0);
    }
}