using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCKComment = ClusterVR.CreatorKit.World.Comment;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class Comment
    {
        public class Source
        {
            public readonly string body;
            public readonly string displayName;
            public readonly string id;
            public readonly double timestamp;
            public readonly CommentVia via;

            public Source(
                CCKComment cckComment,
                string id,
                double timestamp,
                CommentVia via
            )
            {
                this.body = cckComment.Body;
                this.displayName = cckComment.CommentedBy.DisplayName;
                this.id = id;
                this.timestamp = timestamp;
                if (via == CommentVia.YouTube) this.timestamp = Math.Round(this.timestamp / 1000) * 1000;
                this.via = via;
            }
        }

        public string body => source.body;
        public string displayName => source.displayName;
        public string id => source.id;
        public double timestamp => source.timestamp;
        public string via => source.via.ToString();

        public readonly PlayerHandle sender;

        readonly Source source;

        public Comment(
            Source source,
            PlayerHandle sender
        )
        {
            this.source = source;
            this.sender = source.via == CommentVia.YouTube ? null : sender;
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[Comment][{0}][{1}][{2}]", displayName, body, timestamp);
        }
    }
}
