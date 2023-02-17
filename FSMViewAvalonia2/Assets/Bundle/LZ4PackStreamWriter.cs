using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LZ4ps;

using BlockInfo = FSMViewAvalonia2.Assets.Bundle.LZ4PackContext.BlockInfo;

namespace FSMViewAvalonia2.Assets.Bundle;
internal class LZ4PackStreamWriter : Stream
{
    public LZ4PackContext Context { get; private set; }
    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => _canWrite;

    public override long Length => _length;

    public override long Position
    {
        get => _pos;
        set
        {
            if (value == _pos)
                return;
            if (value < _pos)
                throw new NotSupportedException();
            var count = value - _pos;
            Write(null, 0, (int) count);
        }
    }

    private bool _canWrite = true;
    private long _length = 0;
    private int _blockId = 0;
    private readonly byte[] _buffer = new byte[LZ4PackContext.SizePerBlock];
    private int _bufferPos = 0;
    private long _pos = 0;
    private int _taskCount = 0;
    public override void Flush()
    {

    }
    public LZ4PackStreamWriter(LZ4PackContext context)
    {
        Context = context;
    }
    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
    public override void SetLength(long value) => _length = value;
    public void EndWrite()
    {
        if (!_canWrite)
        {
            return;
        }

        _canWrite = false;
        if (_bufferPos == 0)
        {
            return;
        }
        while(_taskCount > 0)
        {
            Thread.Sleep(0);
        }

        Context.Lock.AcquireWriterLock(-1);
        _blockId++;
        if (Context.blocks.Count < _blockId)
        {
            Context.blocks.AddRange(Enumerable.Repeat((BlockInfo) null,
                _blockId - Context.blocks.Count));
        }

        BlockInfo block = Context.blocks[_blockId - 1];
        block ??= Context.blocks[_blockId - 1] = new();

        lock (block)
        {
            if (block.dontCompress)
            {
                block.isCompressed = false;
                block.data = new byte[LZ4PackContext.SizePerBlock];
                Array.Copy(_buffer, block.data, _bufferPos);
                _bufferPos = 0;
            }
        }

        Context.IsDone = true;
        Context.Lock.ReleaseWriterLock();
    }
    public override void Write(byte[] buffer, int offset, int count)
    {
        Write(buffer, offset, count, false);
    }
    public void Write(byte[] buffer, int offset, int count, bool useNull)
    {
        if (!_canWrite)
        {
            throw new InvalidOperationException();
        }

        int endPos = offset + count;
        while (offset < endPos)
        {
            _buffer[_bufferPos++] = useNull ? (byte) 0 : buffer[offset++];
            _pos++;
            if (Context.Length < _pos)
            {
                Context.Length = _pos;
            }

            if (_bufferPos == LZ4PackContext.SizePerBlock)
            {
                _blockId++;
                if (Context.blocks.Count < _blockId)
                {
                    Context.Lock.AcquireWriterLock(-1);
                    Context.blocks.AddRange(Enumerable.Repeat((BlockInfo) null,
                        _blockId - Context.blocks.Count));
                    Context.Lock.ReleaseWriterLock();
                }

                Context.Lock.AcquireReaderLock(-1);
                BlockInfo block = Context.blocks[_blockId - 1];
                if (block == null)
                {
                    LockCookie cookie = Context.Lock.UpgradeToWriterLock(-1);
                    if (Context.blocks[_blockId - 1] == null)
                    {
                        block = new();
                        Context.blocks[_blockId - 1] = block;
                    }
                    Context.Lock.DowngradeFromWriterLock(ref cookie);
                }

                Context.Lock.ReleaseReaderLock();
                byte[] data = new byte[_bufferPos];
                Array.Copy(_buffer, data, _bufferPos);
                block.isCompressed = false;
                block.data = data;
                _bufferPos = 0;
                if (block.dontCompress)
                {
                    lock (block)
                    {
                        if (block.dontCompress)
                        {
                            continue;
                        }
                    }
                }
                Interlocked.Increment(ref _taskCount);
                Task.Run(() =>
                {
                    byte[] result = LZ4Codec.Encode32HC(data, 0, data.Length);
                    lock (block)
                    {
                        if (result != null && result.Length < data.Length && !block.dontCompress)
                        {
                            block.isCompressed = true;
                            block.data = result;
                        } else
                        {
                            block.isCompressed = false;
                            block.data = data;
                        }
                    }
                    Interlocked.Decrement(ref _taskCount);
                });
            }

        }
    }
}
