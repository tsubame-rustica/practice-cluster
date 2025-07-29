using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public interface ILoggerFactory
    {
        ILogger Create(UnityEngine.GameObject gameObject, IProgramStatus programStatus);
    }

    public interface IRunnerOptions
    {
        bool isDebug { get; }
        string pauseFrameKey { get; }
        IExternalCallerOptions externalCallerOptions { get; }
        bool isRayDraw { get; }
    }

    public interface IExternalCallerOptions
    {
        string GetUrl(string id);
    }

    public interface IPlayerViewOptions
    {
        bool isFirstPersonView { get; set; }
        void SetPersonViewChangeable(bool canChange);
    }

    public interface IProductOptions
    {
        bool IsPublicProduct(string productId);
        string GetProductName(string productId);
    }

    public interface ICommentOptions
    {
        EmulateClasses.CommentVia via { get; }
        int GetNextId();
    }

    public interface IEngineApplyBuilder
    {
        EmulateClasses.IPlayerHandleFactory BuildFactory(
            Components.CSEmulatorItemHandler csItemHandler,
            Jint.Engine engine
        );
    }
}
