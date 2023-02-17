using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LZ4ps;

namespace FSMViewAvalonia2.Assets.Bundle;
internal class LZ4PackContext
{
    public const int SizePerBlock = 4096 * 1024;
    public class BlockInfo
    {
        public byte[] data;
        public bool isCompressed;
        public bool dontCompress;

        public byte[] GetData(LZ4PackContext context)
        {
            RETRY:
            while(data == null && !context.IsDone)
            {
                Thread.Sleep(0);
            }

            if(data == null)
            {
                throw new IndexOutOfRangeException();
            }

            lock (this)
            {
                if(data == null)
                {
                    goto RETRY;
                }

                if(!isCompressed)
                {
                    return data;
                }

                data = LZ4Codec.Decode32(data, 0, data.Length, SizePerBlock);
                isCompressed = false;
                return data;
            }
        }
    }
    public List<BlockInfo> blocks = new();
    public bool IsDone { get; set; } = false;
    public ReaderWriterLock Lock { get; init; } = new();
    public long Length { get; set; } = 0;
    public BlockInfo GetBlockById(int id)
    {
        Lock.AcquireReaderLock(-1);
        if(blocks.Count <= id)
        {
            LockCookie cookie = Lock.UpgradeToWriterLock(-1);
            blocks.AddRange(Enumerable.Repeat((BlockInfo) null, id - blocks.Count + 1));
            blocks[id] = new()
            {
                dontCompress = true
            };
            Lock.DowngradeFromWriterLock(ref cookie);
        }

        BlockInfo block = blocks[id];
        if(block == null)
        {
            LockCookie cookie = Lock.UpgradeToWriterLock(-1);
            if (blocks[id] == null)
            {
                block = blocks[id] = new()
                {
                    dontCompress = true
                };
            }
            
            Lock.DowngradeFromWriterLock(ref cookie);
        }

        Lock.ReleaseReaderLock();
        block.dontCompress = true;
        return block;
    }
}
