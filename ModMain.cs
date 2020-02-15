using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// 1. Added the "GooseModdingAPI" project as a reference.
// 2. Compile this.
// 3. Create a folder with this DLL in the root, and *no GooseModdingAPI DLL*
using GooseShared;
using SamEngine;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using MoonSharp.Interpreter;
using System.Drawing;

namespace GooseDesktop
{
    class ModEntryPoint : IMod
    {
        void IMod.Init()
        {
            // Subscribe to whatever events we want
            InjectionPoints.PostRenderEvent += PostRender;
            Init();
        }
        public static GooseEntity goose;
        public void PostRender(GooseEntity goose1, Graphics g)
        {
            goose = goose1;
            for (int i = 0; i < ModScripts.Length; i++) {
                if (ModRunning[i])
                {
                    try
                    {
                        UpdateMod(g, ModScripts[i], i);
                    }
                    catch (Exception e)
                    {
                        DialogResult action = MessageBox.Show(null, "Uh oh! a BIG error occured!!!\n" + e.ToString(), "Quack Lua", MessageBoxButtons.AbortRetryIgnore);
                        if (action == DialogResult.Abort)
                        {
                            Application.Exit();
                        }
                        else if (action == DialogResult.Retry)
                        {
                            PostRender(goose1, g);
                        }
                        else if (action == DialogResult.Ignore)
                        {
                            ModRunning[i] = false;
                        }
                    }
                }
            }
        }

        public static int ModLimit = 30;
        public static Script[] ModScripts = new Script[ModLimit];
        public static bool[] ModRunning = new bool[ModLimit];
        public static string[] ModNames = new string[ModLimit];
        public void HandleRError(ScriptRuntimeException error, int iter)
        {
            ModRunning[iter] = false;
            MessageBox.Show("Honk! An exception has occured in " + ModNames[iter] + "! \n" + error.DecoratedMessage);
        }
        public void HandleSError(SyntaxErrorException error, int iter)
        {
            ModRunning[iter] = false;
            MessageBox.Show("Honk! A syntax errror has occured in " + ModNames[iter] +"! \n" + error.DecoratedMessage);
        }
        public DynValue StartMods()
        {
            ModNames = Directory.GetFiles(@"Assets\Mods", "*.lua");
            for (int i = 0; i < ModNames.Length; i++)
            {
                ModScripts[i] = new Script(CoreModules.Preset_HardSandbox);
                try {
                    try {  ModScripts[i].DoString(File.ReadAllText(ModNames[i])); } catch (IndexOutOfRangeException) {
                        MessageBox.Show("You madman, you actually reached the mod limit!");
                        return null;
                    }
                } catch (ScriptRuntimeException exc) { HandleRError(exc, i); return null; } catch (SyntaxErrorException exc) { HandleSError(exc, i); return null; }
                Table graph = new Table(ModScripts[i]);

                graph["SetGooseProp"] = Lua_SetGooseProp;
                graph["DrawRect"] = Lua_DrawRect;
                graph["DrawText"] = Lua_DrawText;
                graph["MeasureText"] = Lua_MeasureText;
                graph["GetMousePos"] = Lua_GetMousePos;
                graph["GetMouseHeld"] = Lua_GetMouseHeld;
                graph["MessageBox"] = Lua_MessageBox;
                graph["MessageBoxAsk"] = Lua_MessageBoxAsk;
                graph["MessageBoxIcon"] = Lua_MessageBoxIcon;
                graph["MessageBoxIconAsk"] = Lua_MessageBoxIconAsk;
                graph["MessageBoxInput"] = Lua_MessageBoxInput;
                ModScripts.ElementAt(i).Globals["Graphics"] = graph;
                try
                {
                    DynValue ModStartReturn = ModScripts[i].Call(ModScripts[i].Globals.Get("Start"));
                    return ModStartReturn;
                }
                catch (ScriptRuntimeException exc)
                {
                    HandleRError(exc, i);
                }
            }
            return null;
        }
        public static Graphics g;
        public static int maxFPS = 60;
        public void UpdateMod(Graphics g1, Script script, int iter)
        {
            g = g1;
            //SetLuaValues(goosePosition);
            System.Threading.Thread.Sleep(50 / maxFPS - 8);
            try
            {
                if (ModRunning.ElementAt(iter))
                {
                    try {  } catch (ArgumentException) { ModRunning.SetValue(false, iter); }
                    //GetAndSetLuaValues();
                }
                else
                {
                    
                }
            }
            catch (ScriptRuntimeException exc) { HandleRError(exc, iter); }
        }
        Func<string, object> Lua_GetGooseProp = prop =>
        {
            try
            {
                return GetProp(prop);
            }
            catch (NullReferenceException)
            {
                throw new ScriptRuntimeException(new Exception("invalid goose property"));
            }
        };
        Func<string, DynValue, bool> Lua_SetGooseProp = (prop, toset) =>
        {
            SetProp(prop, toset);
            return true;
        };
        Func<DynValue, DynValue, string, bool> Lua_DrawRect = (pos1, pos2, color) =>
        {
            g.FillRectangle(new SolidBrush(Color.FromName(color)), new Rectangle((int)pos1.Table.Get(1).Number, (int)pos1.Table.Get(2).Number, (int)pos2.Table.Get(1).Number, (int)pos2.Table.Get(2).Number));
            return true;
        };
        Func<DynValue, string, string, float, bool> Lua_DrawText = (pos, content, color, size) =>
        {
            g.DrawString(content, new Font("Arial", size, FontStyle.Bold), new SolidBrush(Color.FromName(color)), (float)pos.Table.Get(1).Number, (float)pos.Table.Get(2).Number);
            return true;
        };
        Func<DynValue, string, string, float, DynValue> Lua_MeasureText = (pos, content, color, size) =>
        {
            SizeF stringsize = g.MeasureString(content, new Font("Arial", size, FontStyle.Bold), int.MaxValue);
            return DynValue.NewTable(new Table(null, new DynValue[] { DynValue.NewNumber(stringsize.Width), DynValue.NewNumber(stringsize.Height) }));
        };
        Func<DynValue> Lua_GetMousePos = () =>
        {
            return DynValue.NewTable(new Table(null, new DynValue[] { DynValue.NewNumber(Cursor.Position.X), DynValue.NewNumber(Cursor.Position.Y) }));
        };
        Func<bool> Lua_GetMouseHeld = () =>
        {
            return (Control.MouseButtons == MouseButtons.Left);
        };
        Func<string, string, bool> Lua_MessageBoxIcon = (content, icon) =>
        {
            try
            {
                MessageBox.Show(null, content, "Lua", MessageBoxButtons.OK, (MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), icon));
            }
            catch (ArgumentException)
            {
                throw new ScriptRuntimeException(new Exception("invalid text box icon"));
            }
            return true;
        };
        Func<string, string, bool> Lua_MessageBoxIconAsk = (content, icon) =>
        {
            try
            {
                DialogResult dr = MessageBox.Show(null, content, "Lua", MessageBoxButtons.YesNo, (MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), icon));
                if (dr == DialogResult.Yes)
                {
                    return true;
                }
                else if (dr == DialogResult.No)
                {
                    return false;
                }
            }
            catch (ArgumentException)
            {
                throw new ScriptRuntimeException(new Exception("invalid text box icon"));
            }
            return false;
        };
        Func<string, bool> Lua_MessageBox = (content) =>
        {
            if (dialogboxes < 3)
            {
                dialogboxes++;
                MessageBox.Show(null, content, "Lua", MessageBoxButtons.OK);
                dialogboxes--;
                return true;
            }
            else
            {
                return false;
            }
        };
        Func<string, bool> Lua_MessageBoxAsk = (content) =>
        {
            if (dialogboxes < 3)
            {
                dialogboxes++;
                DialogResult dr = MessageBox.Show(null, content, "Lua", MessageBoxButtons.YesNo);
                dialogboxes--;
                if (dr == DialogResult.Yes)
                {
                    return true;
                }
                else if (dr == DialogResult.No)
                {
                    return false;
                }
            }
            return false;
        };
        static int dialogboxes = 0;
        Func<string, string, string> Lua_MessageBoxInput = (content, defaultval) =>
        {
            var theval = defaultval;
            if (dialogboxes < 3)
            {
                dialogboxes++;
                DialogResult dr = InputBox("Lua", "A Lua mod has requested input:\n" + content, ref theval);
                dialogboxes--;
                return theval;
            }
            else
            {
                return "";
            }
        };
        Func<string, bool> Lua_Print = (prints) =>
        {   
            throw new ScriptRuntimeException(new Exception("cannot call \"print\" in script execution"));
        };

        //input box
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 15, 372, 23);
            textBox.SetBounds(12, 76, 372, 20);
            buttonOk.SetBounds(228, 110, 75, 23);
            buttonCancel.SetBounds(309, 110, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 140);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.BackColor = Color.White;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
        //input box

        /*Func<string, DynValue, bool> Lua_SetGooseProp = (prop, toset) =>
        {
            return GooseConfig.RegisterConfig();
        };*/

        public static object GetProp(string prop)
        {
            var res = (dynamic)typeof(GooseEntity).GetField(prop).GetValue(goose);
            if (res.GetType() == typeof(Vector2))
            {
                res = new DynValue[] { DynValue.NewNumber(res.x), DynValue.NewNumber(res.y) };
            }
            return res;
        }

        public static object SetProp(string prop, DynValue toset)
        {
            object res = toset;
            if (toset.Type == DataType.Table)
            {
                res = new Vector2((float)toset.Table.Get(1).Number, (float)toset.Table.Get(2).Number);
            };
            if (toset.Type == DataType.Number)
            {
                res = (float)toset.Number;
            };
            if (toset.Type == DataType.String)
            {
                res = toset.String;
            };
            try
            {
                typeof(GooseEntity).GetField(prop).SetValue(goose, res);
            }
            catch (FieldAccessException)
            {
                throw new ScriptRuntimeException(new Exception("attempt to set constant goose property"));
            }
            return true;
        }
        public void Init()
        {
            try
            {
                StartMods();
            } catch (Exception e)
            {
                MessageBox.Show("A fatal error has occured. The application cannot continue.\n" + e.ToString());
                Application.Exit();
            }
            for (int i = 0; i < ModRunning.Length; i++) {
                ModRunning[i] = true;
            }
        }
    }
}
