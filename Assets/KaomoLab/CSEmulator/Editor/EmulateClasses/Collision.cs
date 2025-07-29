﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class Collision
    {
        readonly IEnumerable<CollidePoint> _collidePoints;
        //この配列を直接書き換えようという発想はまあ無いだろうという前提
        public CollidePoint[] collidePoints { get => _collidePoints.ToArray(); }

        public readonly EmulateVector3 impulse;
        public readonly EmulateVector3 relativeVelocity;
        readonly ILogger logger;

        readonly HitObject _object;
        public HitObject @object
        {
            get
            {
                logger.Error("Collision.objectは非推奨(deprecate)になりました。");
                return _object;
            }
        }
        public object handle => _object.GetHandle();

        public Collision(
            IEnumerable<CollidePoint> collidePoints,
            EmulateVector3 impulse,
            HitObject hitObject,
            EmulateVector3 relativeVelocity,
            ILogger logger
        )
        {
            this._collidePoints = collidePoints;
            this.impulse = impulse;
            this._object = hitObject;
            this.relativeVelocity = relativeVelocity;
            this.logger = logger;
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            o.collidePoints = collidePoints;
            o.impulse = impulse;
            o.@object = @object; //2.26.0現在残っているのでそのまま
            o.handle = handle;
            o.relativeVelocity = relativeVelocity;
            return o;
        }
        public override string ToString()
        {
            return String.Format("[Collision][{0}][{1}][{2}][{3}]", collidePoints, impulse, handle, relativeVelocity);
        }
    }
}
