using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using MoonSharp.Interpreter;
using SamEngine;
using System.Drawing;

namespace GooseDesktop
{
    class GooseMods
    {
        public static string ModContent = "";
        public static Script ModScript = new Script(CoreModules.Preset_HardSandbox);
        public GooseMods()
        {

        }
        public void SetLuaValues(Vector2 goosePosition)
        {
            ModScript.Globals.Set("GoosePosX", DynValue.NewNumber(goosePosition.x));
            ModScript.Globals.Set("GoosePosY", DynValue.NewNumber(goosePosition.y));
            ModScript.Globals.Set("GooseRot", DynValue.NewNumber(TheGoose.direction));
            ModScript.Globals.Set("GooseVelX", DynValue.NewNumber(TheGoose.velocity.x));
            ModScript.Globals.Set("GooseVelY", DynValue.NewNumber(TheGoose.velocity.y));
        }
        public void GetAndSetLuaValues()
        {
            TheGoose.SetPos(new Vector2((float)ModScript.Globals.Get("GoosePosX").Number, (float)ModScript.Globals.Get("GoosePosY").Number));
            TheGoose.SetVel(new Vector2((float)ModScript.Globals.Get("GooseVelX").Number, (float)ModScript.Globals.Get("GooseVelY").Number));
            TheGoose.SetRot((float)ModScript.Globals.Get("GooseRot").Number);
        }
        public void HandleRError(ScriptRuntimeException error)
        {
            TheGoose.ModRunning = false;
            MessageBox.Show("Honk! An exception has occured in the loaded mod! \n" + error.DecoratedMessage);
        }
        public void HandleSError(SyntaxErrorException error)
        {
            TheGoose.ModRunning = false;
            MessageBox.Show("Honk! A syntax errror has occured in the loaded mod! \n" + error.DecoratedMessage);
        }
        public DynValue StartMods()
        {
            try { DynValue ModLoadReturn = ModScript.DoString(ModContent); } catch (ScriptRuntimeException exc) { HandleRError(exc); return null; } catch (SyntaxErrorException exc) { HandleSError(exc); return null; }
            ModScript.Globals["GetGooseProp"] = Lua_GetGooseProp;
            ModScript.Globals["SetGooseProp"] = Lua_SetGooseProp;
            ModScript.Globals["DrawRect"] = Lua_DrawRect;
            ModScript.Globals["DrawText"] = Lua_DrawText;
            ModScript.Globals["MeasureText"] = Lua_MeasureText;
            ModScript.Globals["GetMousePos"] = Lua_GetMousePos;
            ModScript.Globals["GetMouseHeld"] = Lua_GetMouseHeld;
            ModScript.Globals["MessageBox"] = Lua_MessageBox;
            ModScript.Globals["MessageBoxAsk"] = Lua_MessageBoxAsk;
            ModScript.Globals["MessageBoxIcon"] = Lua_MessageBoxIcon;
            ModScript.Globals["MessageBoxIconAsk"] = Lua_MessageBoxIconAsk;
            ModScript.Globals["MessageBoxInput"] = Lua_MessageBoxInput;
            try
            {
                DynValue ModStartReturn = ModScript.Call(ModScript.Globals.Get("Start"));
                return ModStartReturn;
            }
            catch (ScriptRuntimeException exc)
            {
                HandleRError(exc);
            }
            return null;
        }
        public static Graphics g;
        public DynValue UpdateMods(Graphics g1)
        {
            g = g1;
            //SetLuaValues(goosePosition);
            try
            {
                if (TheGoose.ModRunning)
                {
                    try { DynValue ModUpdReturn = ModScript.Call(ModScript.Globals.Get("Update")); return ModUpdReturn; } catch (ArgumentException) { TheGoose.ModRunning = false; return null; }
                    //GetAndSetLuaValues();
                }
                else
                {
                    return null;
                }
            }
            catch (ScriptRuntimeException exc) { HandleRError(exc); return null; }
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
            return DynValue.NewTable(new Table(ModScript, new DynValue[] { DynValue.NewNumber(stringsize.Width), DynValue.NewNumber(stringsize.Height) }));
        };
        Func<DynValue> Lua_GetMousePos = () =>
        {
            return DynValue.NewTable(new Table(ModScript, new DynValue[] { DynValue.NewNumber(Cursor.Position.X), DynValue.NewNumber(Cursor.Position.Y) }));
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

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 56, 372, 20);
            buttonOk.SetBounds(228, 100, 75, 23);
            buttonCancel.SetBounds(309, 100, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 140);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
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
            var res = (dynamic)typeof(TheGoose).GetField(prop).GetValue(null);
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
                typeof(TheGoose).GetField(prop).SetValue(null, res);
            }
            catch (FieldAccessException)
            {
                throw new ScriptRuntimeException(new Exception("attempt to set constant goose property"));
            }
            return true;
        }




        public static GooseConfig.ConfigSettings ReadFileIntoConfig(string configGivenPath)
        {
            GooseConfig.ConfigSettings configSettings = new GooseConfig.ConfigSettings();
            if (!File.Exists(configGivenPath))
            {
                MessageBox.Show("Can't find config.goos file! Creating a new one with default values");
                GooseConfig.ConfigSettings.WriteConfigToFile(configGivenPath, configSettings);
                return configSettings;
            }
            try
            {
                using (StreamReader streamReader = new StreamReader(configGivenPath))
                {
                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                    string text;
                    while ((text = streamReader.ReadLine()) != null)
                    {
                        string[] array = text.Split(new char[]
                        {
                                '='
                        });
                        if (array.Length == 2)
                        {
                            dictionary.Add(array[0], array[1]);
                        }
                    }
                    int num = -1;
                    int.TryParse(dictionary["Version"], out num);
                    if (num != 0)
                    {
                        MessageBox.Show("mod.goos is for the wrong mod! Creating a new one with default values!");
                        File.Delete(configGivenPath);
                        GooseConfig.ConfigSettings.WriteConfigToFile(configGivenPath, configSettings);
                        return configSettings;
                    }
                    foreach (KeyValuePair<string, string> keyValuePair in dictionary)
                    {
                        FieldInfo field = typeof(GooseConfig.ConfigSettings).GetField(keyValuePair.Key);
                        try
                        {
                            field.SetValue(configSettings, Convert.ChangeType(keyValuePair.Value, field.FieldType));
                        }
                        catch
                        {
                            MessageBox.Show("Loading config error: field " + field.Name + "'s value is not valid. Setting it to the default value.");
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("mod.goos corrupt! Creating a new one!");
                File.Delete(configGivenPath);
                GooseConfig.ConfigSettings.WriteConfigToFile(configGivenPath, configSettings);
                return configSettings;
            }
            return configSettings;
        }

        // Token: 0x06000040 RID: 64 RVA: 0x000031D8 File Offset: 0x000013D8
        public static void WriteConfigToFile(string path, GooseConfig.ConfigSettings f)
        {
            using (StreamWriter streamWriter = File.CreateText(path))
            {
                streamWriter.Write(GooseConfig.ConfigSettings.GenerateTextFromSettings(f));
            }
        }

        // Token: 0x06000041 RID: 65 RVA: 0x00003214 File Offset: 0x00001414
        public static string GenerateTextFromSettings(GooseConfig.ConfigSettings f)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (FieldInfo fieldInfo in typeof(GooseConfig.ConfigSettings).GetFields())
            {
                stringBuilder.Append(string.Format("{0}={1}\n", fieldInfo.Name, fieldInfo.GetValue(f).ToString()));
            }
            return stringBuilder.ToString();
        }

        public static GooseConfig.ConfigSettings settings = null;




        public void Init()
        {
            if (File.Exists(Program.GetPathToFileInAssembly(@"mod.lua")))
            {
                ModContent = File.ReadAllText(Program.GetPathToFileInAssembly(@"mod.lua"));
                StartMods();
                TheGoose.ModRunning = true;
            }
        }
    }
}
