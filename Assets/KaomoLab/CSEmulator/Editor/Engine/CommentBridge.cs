using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class CommentBridge
        : ICommentHandler, ClusterVR.CreatorKit.World.ICommentScreenView
    {
        readonly List<Comment.Source> comments = new();

        Dictionary<ulong, IJintCallback<IReadOnlyList<Comment.Source>>> callbacks = new();

        readonly ICommentOptions options;

        public CommentBridge(
            ICommentOptions options
        )
        {
            this.options = options;
        }

        public IReadOnlyList<Comment.Source> getLatestComments(int count)
        {
            return comments.Skip(comments.Count - Math.Clamp(count, 0, 100)).ToList();
        }

        public void SetCommentReceivedCallback(ulong itemId, IJintCallback<IReadOnlyList<Comment.Source>> Callback)
        {
            callbacks[itemId] = Callback;
        }

        public void RemoveCommentReceivedCallback(ulong itemId)
        {
            callbacks.Remove(itemId);
        }


        event Action ClusterVR.CreatorKit.World.ICommentScreenView.OnDestroyed
        {
            //nop
            add { }
            remove { }
        }

        void ClusterVR.CreatorKit.World.ICommentScreenView.AddComment(ClusterVR.CreatorKit.World.Comment comment)
        {
            var id = options.GetNextId();
            var timestamp = CSEmulator.Commons.UnixEpochMs();
            var via = options.via;
            var c = new Comment.Source(comment, id.ToString(), timestamp, via);
            comments.Add(c);

            foreach(var callback in callbacks.Values)
            {
                callback.Execute(new List<Comment.Source>(){ c });
            }
        }

        void ClusterVR.CreatorKit.World.ICommentScreenView.RemoveComment(string commentId)
        {
            //nop
        }
    }
}
