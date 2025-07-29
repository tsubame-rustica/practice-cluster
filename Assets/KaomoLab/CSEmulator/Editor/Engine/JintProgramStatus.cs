using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class JintProgramStatus
        : IProgramStatus
    {
        public static string FormattedLocation(Acornima.SourceLocation? location)
        {
            if (!location.HasValue) return "";
            return String.Format(
                "{1}:{2}",
                location.Value.SourceFile,
                location.Value.Start.Line,
                location.Value.Start.Column
            );
        }

        readonly Jint.Engine engine;

        public JintProgramStatus(
            Jint.Engine engine
        )
        {
            this.engine = engine;
        }

        public string GetLineInfo()
        {
            return FormattedLocation(engine.Debugger.CurrentLocation);
        }

        public string GetStack()
        {
            //(C#のExceptionを処理するシーンでは)道中でResetされているらしく、stackの情報はあるがsizeが0になっているため、stacktraceが生成されない。
            //そのため無理やりsizeを更新してstackを生成させている。
            var jintCallStack = typeof(Jint.Engine).GetField("CallStack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(engine);
            var refStack = jintCallStack.GetType().GetField("_stack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(jintCallStack);
            var array = refStack.GetType().GetField("_array", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(refStack);
            var sizeField = refStack.GetType().GetField("_size", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var size = (int)sizeField.GetValue(refStack);
            if(size != 0)
            {
                //sizeがある場合はstacktraceを出せるのでそのまま。
                var stack = engine.Advanced.StackTrace;
                return stack;
            }
            else
            {
                //sizeを上書きして出力させる。
                sizeField.SetValue(refStack, ((Array)array).Length);
                try
                {
                    var stack = engine.Advanced.StackTrace;
                    return stack;
                }
                catch (NullReferenceException)
                {
                    return ""; //取得失敗
                }
                finally
                {
                    sizeField.SetValue(refStack, 0);
                }
            }
        }
    }
}
