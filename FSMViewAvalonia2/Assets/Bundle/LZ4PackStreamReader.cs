using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2.Assets.Bundle;
internal class LZ4PackStreamReader : Stream
{
    public LZ4PackContext Context { get; private set; }
    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => Context.IsDone ? Context.Length : long.MaxValue;

    public override long Position { get; set; }
    public LZ4PackStreamReader(LZ4PackContext context)
    {
        Context = context;
    }

    public override void Flush() => throw new NotImplementedException();
    public override int Read(byte[] buffer, int offset, int count)
    {
        long endPos = Position + count;
        byte[] lastBlock = null;
        long lastBlockId = -1;
        int r_count = 0;
        while (Position < endPos)
        {
            if(Position >= Length && Context.IsDone)
            {
                break;
            }

            long blockId = Position / LZ4PackContext.SizePerBlock;
            if(blockId != lastBlockId || lastBlock == null)
            {
                lastBlockId = blockId;
                lastBlock = Context.GetBlockById((int) blockId).GetData(Context);
            }

            long d_offset = Position % LZ4PackContext.SizePerBlock;
            buffer[offset++] = lastBlock[d_offset];
            r_count++;
            Position++;
        }

        return r_count;
    }
    public override long Seek(long offset, SeekOrigin origin)
    {
        return Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            _ => throw new NotImplementedException()
        };
    }
    public override void SetLength(long value) => throw new NotImplementedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
}
