using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UltimateReplay.Storage
{
    public sealed class ReplayMemoryStorage : ReplayStorage
    {
        // Type
        private struct SnapshotEntry
        {
            // Public
            public float TimeStamp;
            public int SequenceID;
            public int DataOffset;
            public int DataLength;
        }

        // Private
        private float duration = 0f;
        private float rollingBufferDuration = -1f;
        private int memorySize = 0;
        private int identitySize = sizeof(ushort);
        private MemoryStream snapshotData = new MemoryStream();
        private List<SnapshotEntry> snapshotEntries = new List<SnapshotEntry>(256);
        private BinaryWriter snapshotWriter = null;
        private BinaryReader snapshotReader = null;

        // Properties
        public override bool CanRead
        {
            get
            {
                CheckDisposed();
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                CheckDisposed();
                return true;
            }
        }

        public override float Duration
        {
            get
            {
                CheckDisposed();
                return duration;
            }
        }

        public override int MemorySize
        {
            get
            {
                CheckDisposed();
                return memorySize;
            }
        }

        public override int SnapshotSize
        {
            get
            {
                CheckDisposed();
                return snapshotEntries.Count;
            }
        }

        public override int IdentitySize
        {
            get
            {
                CheckDisposed();
                return identitySize;
            }
        }

        public float RollingBufferDuration
        {
            get
            {
                CheckDisposed();
                return rollingBufferDuration;
            }
        }

        // Constructor
        public ReplayMemoryStorage(string replayName = null, float rollingBufferDuration = -1f)
            : base(replayName)
        {
            // Get size of identity
            identitySize = ReplayIdentity.byteSize;

            // Store buffer size
            this.rollingBufferDuration = rollingBufferDuration;
        }

        // Methods
        public override ReplaySnapshot FetchSnapshot(float timeStamp)
        {
            // Check for disposed
            CheckDisposed();

            // Check for no data
            if (snapshotEntries.Count == 0)
                return null;

            SnapshotEntry entry = default;

            // Check for last clip
            if (timeStamp >= duration)
            {
                // Get the very last snapshot
                entry = snapshotEntries[snapshotEntries.Count - 1];
            }
            else
            {
                foreach (SnapshotEntry snapshotEntry in snapshotEntries)
                {
                    if (timeStamp >= snapshotEntry.TimeStamp)
                    {
                        entry = snapshotEntry;
                        //break;

                        // We can stop searching at this point
                        if (timeStamp <= snapshotEntry.TimeStamp)
                            break;
                    }
                }

                // Check for default - sequence id will b e zero if default value is used, as 1 is the min normal value - saves a call to Equals which requires boxing
                if (entry.SequenceID == 0)
                    return null;
            }

            // Move to offset
            snapshotData.Seek(entry.DataOffset, SeekOrigin.Begin);

            // Get the snapshot
            ReplaySnapshot state = ReplaySnapshot.pool.GetReusable();
            state.OnReplayStreamDeserialize(snapshotReader);

            // Patch snapshot values - it won't be correct if we are using a rolling buffer
            state.TimeStamp = entry.TimeStamp;
            state.SequenceID = entry.SequenceID;

            // No snapshot found
            return state;
        }

        public override ReplaySnapshot FetchSnapshot(int sequenceID)
        {
            // Check for disposed
            CheckDisposed();

            // Check for no segments
            if (snapshotEntries.Count == 0 || sequenceID < 1 || sequenceID >= snapshotEntries.Count)
                return null;

            // Get entry
            SnapshotEntry entry = snapshotEntries[sequenceID - 1];

            // Move to offset
            snapshotData.Seek(entry.DataOffset, SeekOrigin.Begin);

            // Get the snapshot
            ReplaySnapshot state = ReplaySnapshot.pool.GetReusable();
            state.OnReplayStreamDeserialize(snapshotReader);

            // Patch snapshot values - it won't be correct if we are using a rolling buffer
            state.TimeStamp = entry.TimeStamp;
            state.SequenceID = entry.SequenceID;

            // No snapshot found
            return state;
        }

        public override void StoreSnapshot(ReplaySnapshot state)
        {
            // Check for disposed
            CheckDisposed();

            // Get offset
            int offset = (int)snapshotData.Position;

            // Write to stream
            state.OnReplayStreamSerialize(snapshotWriter);

            // Get size of stored data
            int size = (int)snapshotData.Position - offset;

            // Add entry
            snapshotEntries.Add(new SnapshotEntry
            {
                DataOffset = offset,
                DataLength = size,
                TimeStamp = state.TimeStamp,
                SequenceID = state.SequenceID,
            });


            // Update duration
            duration = state.TimeStamp;

            // Constrain buffer for rolling buffer support
            if (rollingBufferDuration > 0f && duration > rollingBufferDuration)
            {
                // Constrain buffer
                ConstrainBuffer();

                // Update duration
                duration = snapshotEntries[snapshotEntries.Count - 1].TimeStamp - snapshotEntries[0].TimeStamp;
            }

            // Recycle the snapshot
            state.Dispose();
        }

        public override void Prepare(ReplayStorageAction mode)
        {
            // Check for disposed
            CheckDisposed();

            switch (mode)
            {
                case ReplayStorageAction.Discard:
                    {
                        snapshotData.Dispose();
                        snapshotData = new MemoryStream(256);

                        // Check for writer
                        if (snapshotWriter != null)
                        {
                            snapshotWriter.Dispose();
                            snapshotWriter = null;
                        }

                        // Check for reader
                        if (snapshotReader != null)
                        {
                            snapshotReader.Dispose();
                            snapshotReader = null;
                        }

                        // Clear entries
                        snapshotEntries.Clear();

                        // Clear all persistent data
                        persistentData = new ReplayPersistentData();

                        // Reset values
                        duration = 0f;
                        memorySize = 0;
                        break;
                    }
                case ReplayStorageAction.Write:
                    {
                        // Check for already written to
                        if (snapshotEntries.Count > 0)
                            throw new InvalidOperationException("The memory storage target already has data store. You must clear the data to begin new writing operations");

                        // Release reader
                        if (snapshotReader != null)
                        {
                            snapshotReader.Dispose();
                            snapshotReader = null;
                        }

                        // Create writer
                        snapshotWriter = new BinaryWriter(snapshotData, Encoding.UTF8, true);
                        break;
                    }
                case ReplayStorageAction.Read:
                    {
                        // Release writer
                        if (snapshotWriter != null)
                        {
                            snapshotWriter.Dispose();
                            snapshotWriter = null;
                        }

                        // Create reader
                        snapshotReader = new BinaryReader(snapshotData, Encoding.UTF8, true);
                        break;
                    }

                case ReplayStorageAction.Commit:
                    {
                        // Check for any segments
                        if (snapshotEntries.Count == 0)
                            break;

                        // Get first segment
                        SnapshotEntry first = snapshotEntries[0];

                        // Calculate offset values
                        float offsetTime = first.TimeStamp;
                        int offsetSequenceId = first.SequenceID - 1;

                        // Update all stored snapshots so that offsets and sequence ids start from appropriate values
                        for (int i = 0; i < snapshotEntries.Count; i++)
                        {
                            SnapshotEntry entry = snapshotEntries[i];

                            entry.TimeStamp -= offsetTime;
                            entry.SequenceID -= offsetSequenceId;

                            snapshotEntries[i] = entry;
                        }
                        break;
                    }
            }
        }

        private void ConstrainBuffer()
        {
            // Check for segments
            if (snapshotEntries.Count > 0)
            {
                // Get the first segment
                SnapshotEntry entry = snapshotEntries[0];

                // Remove entry
                snapshotEntries.RemoveAt(0);

                // Trim the leading buffer
                byte[] buffer = snapshotData.GetBuffer();
                Buffer.BlockCopy(buffer, entry.DataLength, buffer, 0, (int)snapshotData.Length - entry.DataLength);
                snapshotData.Position -= entry.DataLength;

                // Update offsets
                for (int i = 0; i < snapshotEntries.Count; i++)
                {
                    SnapshotEntry updateEntry = snapshotEntries[i];

                    updateEntry.DataOffset -= entry.DataLength;

                    snapshotEntries[i] = updateEntry;
                }
            }
        }

        protected override void OnDispose()
        {
            snapshotData.Dispose();
            snapshotData = null;

            // Check for writer
            if (snapshotWriter != null)
            {
                snapshotWriter.Dispose();
                snapshotWriter = null;
            }

            // Check for reader
            if (snapshotReader != null)
            {
                snapshotReader.Dispose();
                snapshotReader = null;
            }

            // Clear entries
            snapshotEntries.Clear();

            duration = 0;
            memorySize = 0;
        }

        #region Serialization
        public bool SaveToFile(string replayFile)
        {
            // Create file storage
            using (ReplayFileStorage fileStorage = ReplayFileStorage.FromFile(replayFile))
            {
                // Copy to
                return CopyTo(fileStorage);
            }
        }

        public ReplayAsyncOperation SaveToFileAsync(string replayFile)
        {
            // Create file storage
            using (ReplayFileStorage fileStorage = ReplayFileStorage.FromFile(replayFile))
            {
                // Copy to async
                return CopyToAsync(fileStorage);
            }
        }

        public bool LoadFromFile(string replayFile)
        {
            // Create file storage
            using (ReplayFileStorage fileStorage = ReplayFileStorage.FromFile(replayFile))
            {
                // Copy from file
                return fileStorage.CopyTo(this);
            }
        }

        public ReplayAsyncOperation LoadFromFileAsync(string replayFile)
        {
            // Create file storage
            using (ReplayFileStorage fileStorage = ReplayFileStorage.FromFile(replayFile))
            {
                // Copy from file async
                return fileStorage.CopyToAsync(this);
            }
        }

        public byte[] ToBytes()
        {
            // Create memory stream target
            using (MemoryStream stream = new MemoryStream())
            {
                // Create replay stream storage
                using (ReplayStreamStorage streamStorage = ReplayStreamStorage.FromStream(stream))
                {
                    // Copy to
                    if (CopyTo(streamStorage) == false)
                        return Array.Empty<byte>();
                }

                // Get stream bytes
                return stream.ToArray();
            }
        }

        public bool FromBytes(byte[] bytes)
        {
            // Create memory stream input
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                // Create replay stream storage
                using (ReplayStreamStorage streamStorage = ReplayStreamStorage.FromStream(stream))
                {
                    // Copy from bytes
                    return streamStorage.CopyTo(this);
                }
            }
        }
        #endregion
    }
}
