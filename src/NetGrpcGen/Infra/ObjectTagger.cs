using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NetGrpcGen.Infra
{
    internal class ObjectId : IDisposable
    {
        private readonly ObjectTagger _tagger;

        internal ObjectId(ObjectTagger tagger)
            : this(0, tagger)
        {
        }

        internal ObjectId(UInt64 id, ObjectTagger tagger)
        {
            Id = id;
            _tagger = tagger;
        }

        private UInt64 Id { get; set; }

        public static implicit operator UInt64(ObjectId oId)
        {
            return oId.Id;
        }

        public void Dispose()
        {
            _tagger?.FreeId(Id);
        }

        ~ObjectId()
        {
            _tagger?.FreeId(Id);
        }
    }
    
    internal class ObjectTagger
    {
        internal static ObjectTagger Default { get; } = new ObjectTagger();
        private readonly ConditionalWeakTable<object, ObjectId> _objectIdRefs = new ConditionalWeakTable<object, ObjectId>();
        private UInt64 _MaxId = UInt64.MaxValue - 1;

        private ObjectTagger(UInt64 maxId = UInt64.MaxValue - 1)
        {
            _MaxId = maxId;
        }

        internal UInt64 GetOrCreateTag(object obj)
        {
            var result = GetTag(obj);
            if (result.HasValue)
            {
                return result.Value;
            }
            var newObjId = CreateNewObjectId();
            _objectIdRefs.Add(obj, newObjId);
            return newObjId;
        }

        internal UInt64? GetTag(object obj)
        {
            if (_objectIdRefs.TryGetValue(obj, out var objId))
            {
                return objId;
            }
            return null;
        }

        private ObjectId CreateNewObjectId()
        {
            return new ObjectId(TakeNextFreeId(), this);
        }

        #region Id management

        private UInt64 NextId = 1;
        private HashSet<UInt64> UsedIds = new HashSet<UInt64>();

        private UInt64 TakeNextFreeId()
        {
            lock (this)
            {
                UInt64 nextId = NextId;
                UsedIds.Add(nextId);
                NextId = CalculateNextFreeId(nextId);
                return nextId;
            }
        }

        private UInt64 CalculateNextFreeId(UInt64 nextId)
        {
            bool firstPass = true;
            while (UsedIds.Contains(nextId))
            {
                if (nextId >= _MaxId)
                {
                    if (!firstPass)
                    {
                        throw new Exception("Too many object ids in use!");
                    }
                    nextId = 1;
                    firstPass = false;
                }
                else
                {
                    nextId++;
                }
            }
            return nextId;
        }

        internal void FreeId(UInt64 id)
        {
            lock (this)
            {
                UsedIds.Remove(id);
            }
        }

        #endregion
    }
    
    internal static class ObjectTaggerExtension
    {
        public static UInt64 GetOrCreateTag(this object obj)
        {
            return ObjectTagger.Default.GetOrCreateTag(obj);
        }

        public static UInt64? GetTag(this object obj)
        {
            return ObjectTagger.Default.GetTag(obj);
        }
    }
}