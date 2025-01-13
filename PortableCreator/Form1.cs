using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using PortableCreator.Properties;
using TsudaKageyu;

namespace PortableCreator
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			this.InitializeComponent();
		}

		private void CreateBtn_Click(object sender, EventArgs e)
		{
			this.sourceCode = this.txtSource.Text;
			if (this.projectName.Text.Length < 1 || this.exePath.Text.Length < 3)
			{
				MessageBox.Show("Имя проекта или Путь к основному *.exe файлу приложения не могут быть пустыми.", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			this.txtSource.Text = this.txtSource.Text.Replace("Text = \"Form1\";", "Text = \"" + ((this.projectName.Text.Length < 1) ? "Generated" : this.projectName.Text) + "\";");
			this.txtSource.Text = this.txtSource.Text.Replace("\"PortableCreator\"", "\"" + ((this.projectName.Text.Length < 1) ? "Generated" : this.projectName.Text) + "\"");
			this.CheckData();
			this.txtSource.Text = this.sourceCode;
		}

		private void CheckData()
		{
			if (this.exePath.Text.Contains("\\Program Files\\"))
			{
				this.exePathDone = this.exePath.Text.Replace("\"", "").Replace(this.appPath + "Data\\Program Files", "C:\\Program Files");
				if (!this.exePathDone.EndsWith(".exe"))
				{
					this.exePathDone += ".exe";
				}
				if (!File.Exists(this.exePathDone) && !File.Exists(this.exePath.Text.Replace("\"", "")))
				{
					MessageBox.Show("Этот .exe файл отсутствует по указанному пути:\r\n\"" + this.exePathDone + "\"", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				this.txtSource.Text = this.txtSource.Text.Replace("appPath + @\"\\EXEPATH\"", "@\"" + this.exePathDone + "\"");
			}
			else if (this.exePath.Text.Contains("\\Program Files (x86)\\"))
			{
				this.exePathDone = this.exePath.Text.Replace("\"", "").Replace(this.appPath + "Data\\Program Files (x86)", "C:\\Program Files (x86)");
				if (!this.exePathDone.EndsWith(".exe"))
				{
					this.exePathDone += ".exe";
				}
				if (!File.Exists(this.exePathDone) && !File.Exists(this.exePath.Text.Replace("\"", "")))
				{
					MessageBox.Show("Этот .exe файл отсутствует по указанному пути:\r\n\"" + this.exePathDone + "\"", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				this.txtSource.Text = this.txtSource.Text.Replace("appPath + @\"\\EXEPATH\"", "@\"" + this.exePathDone + "\"");
			}
			else
			{
				this.exePathDone = this.exePath.Text.Replace("\"", "").Replace(this.appPath, "");
				if (!this.exePathDone.EndsWith(".exe"))
				{
					this.exePathDone += ".exe";
				}
				if (!File.Exists(this.exePathDone) && !File.Exists(this.exePath.Text.Replace("\"", "")))
				{
					MessageBox.Show("Этот .exe файл отсутствует по указанному пути:\r\n\"" + this.exePathDone + "\"", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				this.txtSource.Text = this.txtSource.Text.Replace("EXEPATH", this.exePathDone);
				if (File.Exists(exePathDone))
				{
					FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(this.exePathDone);
					string exeVersion = versionInfo.FileVersion;
					if (exeVersion != "")
					{
						this.txtSource.Text = this.txtSource.Text.Replace("\"2.4.0.0\"", "\"" + exeVersion + "\"");
					}
				}
			}
			if (this.clearLogs.Checked)
			{
				this.txtSource.Text = this.txtSource.Text.Replace("//ClearLogs", "try { Directory.Delete(userPath + @\"\\AppData\\Local\\Microsoft\\CLR_v4.0\", true); Directory.Delete(userPath + @\"\\AppData\\Local\\Microsoft\\CLR_v4.0_32\", true); } catch {}");
			}
			if (this.customArgs.Text.Length > 1)
			{
				string text = this.customArgs.Text.Replace("\"", "\\\"");
				this.txtSource.Text = this.txtSource.Text.Replace(", ar);", ", \"" + text + " \" + ar);");
			}
			this.Generate();
		}

		private bool IsDirectoryEmpty(string path)
		{
			return !Directory.EnumerateFileSystemEntries(path).Any<string>();
		}

		private void Generate()
		{
			using (FileStream fileStream = new FileStream(this.appPath + "icon.ico", FileMode.CreateNew))
			{
				try
				{
					if (!File.Exists(this.exePathDone))
					{
						new IconExtractor(this.exePath.Text.Replace("\"", "")).GetIcon(0).Save(fileStream);
					}
					else
					{
						new IconExtractor(this.exePathDone).GetIcon(0).Save(fileStream);
					}
				}
				catch
				{
				}
			}
			this.txtStatus.Clear();
			using (ResXResourceWriter resXResourceWriter = new ResXResourceWriter("StringResource.resx"))
			{
				resXResourceWriter.AddResource("executablePath\u200e", this.exePathDone);
				resXResourceWriter.AddResource("executableArgs\u200e", this.customArgs.Text);
			}
			string text = this.appPath + ((this.projectName.Text.Length < 1) ? "Generated" : this.projectName.Text) + ".exe";
			string[] array = new string[] { "System.dll", "System.Drawing.dll", "System.Windows.Forms.dll" };
			CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
			CompilerParameters compilerParameters = new CompilerParameters(array, "");
			compilerParameters.ReferencedAssemblies.Add("System.Linq.dll");
			compilerParameters.ReferencedAssemblies.Add("System.Core.dll");
			compilerParameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
			compilerParameters.ReferencedAssemblies.Add("System.Runtime.InteropServices.dll");
			string text2 = this.appPath + "System.Management.Automation.dll";
			File.WriteAllBytes(text2, Resources.System_Management_Automation);
			compilerParameters.ReferencedAssemblies.Add("System.Management.Automation.dll");
			compilerParameters.OutputAssembly = text;
			compilerParameters.GenerateExecutable = true;
			compilerParameters.GenerateInMemory = true;
			compilerParameters.WarningLevel = 3;
			compilerParameters.TreatWarningsAsErrors = true;
			compilerParameters.EmbeddedResources.Add("StringResource.resx");
			if (File.Exists(this.appPath + "icon.ico"))
			{
				if (new FileInfo(this.appPath + "icon.ico").Length == 0L)
				{
					compilerParameters.CompilerOptions = "/optimize /target:winexe";
					File.Delete(this.appPath + "icon.ico");
				}
				else
				{
					compilerParameters.CompilerOptions = "/optimize /target:winexe /win32icon:\"" + this.appPath + "icon.ico\"";
				}
			}
			string text3 = null;
			try
			{
				CompilerResults compilerResults = codeDomProvider.CompileAssemblyFromSource(compilerParameters, new string[] { this.txtSource.Text });
				if (compilerResults.Errors.Count > 0)
				{
					text3 = "";
					foreach (object obj in compilerResults.Errors)
					{
						CompilerError compilerError = (CompilerError)obj;
						text3 = string.Concat(new string[]
						{
							text3,
							"Line number ",
							compilerError.Line.ToString(),
							", Error Number: ",
							compilerError.ErrorNumber,
							", '",
							compilerError.ErrorText,
							";\r\n\r\n"
						});
					}
				}
			}
			catch (Exception ex)
			{
				text3 = ex.Message;
			}
			if (text3 == null)
			{
				this.txtStatus.Text = "\n\r" + text + " Compiled !";
				File.Delete(this.appPath + "icon.ico");
				if (File.Exists(this.appPath + "log.txt"))
				{
					File.Delete(this.appPath + "log.txt");
				}
				string text4 = this.appPath + "crc.exe";
				File.WriteAllBytes(text4, Resources.crc);
				Process process = new Process
				{
					StartInfo = 
					{
						FileName = "crc",
						WorkingDirectory = this.appPath,
						Arguments = string.Concat(new string[]
						{
							"\"",
							this.appPath,
							"\\",
							this.projectName.Text,
							".exe\""
						}),
						CreateNoWindow = true,
						UseShellExecute = false
					}
				};
				process.Start();
				process.WaitForExit();
				File.Delete(text4);
				File.Delete(text2);
				File.Delete(this.appPath + "StringResource.resx");
				MessageBox.Show("Лаунчер успешно создан!", "Готово!", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				this.txtStatus.Text = "Error occurred during compilation : \r\n" + text3;
				File.Delete(this.appPath + "icon.ico");
				MessageBox.Show(this.txtStatus.Text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			this.txtSource.Text = this.sourceCode;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			new List<string>
			{
				this.appPath + "App",
				this.appPath + "Data\\AppData\\Local",
				this.appPath + "Data\\AppData\\LocalLow",
				this.appPath + "Data\\AppData\\Roaming",
				this.appPath + "Data\\ProgramData",
				this.appPath + "Data\\Program Files",
				this.appPath + "Data\\Program Files (x86)",
				this.appPath + "Data\\Program Files (x86)\\Common Files",
				this.appPath + "Data\\Program Files\\Common Files",
				this.appPath + "Data\\Пользователи\\Видео",
				this.appPath + "Data\\Пользователи\\Документы",
				this.appPath + "Data\\Пользователи\\Общие\\Документы",
				this.appPath + "Data\\Пользователи\\Загрузки",
				this.appPath + "Data\\Пользователи\\Изображения",
				this.appPath + "Data\\Пользователи\\Музыка",
				this.appPath + "Data\\Пользователи\\Рабочий стол",
				this.appPath + "Data\\Реестр",
				this.appPath + "Data\\Драйвер"
			}.AsParallel<string>().ForAll(delegate(string x)
			{
				if (!Directory.Exists(x))
				{
					Directory.CreateDirectory(x);
				}
			});
			new List<string>
			{
				this.appPath + "Data\\Закрыть.txt",
				this.appPath + "Data\\Реестр\\Удалить.txt",
				this.appPath + "Data\\Проводник.txt",
				this.appPath + "Data\\Кастом.txt",
				this.appPath + "Data\\Службы.txt"
			}.AsParallel<string>().ForAll(delegate(string x)
			{
				if (!File.Exists(x))
				{
					this.WriteAsStream(x, "", false);
				}
			});
		}

		private void WriteAsStream(string path, string data, bool append)
		{
			if (append)
			{
				using (StreamWriter streamWriter = new StreamWriter(path, true))
				{
					streamWriter.WriteLine(data, Encoding.UTF8);
					return;
				}
			}
			using (StreamWriter streamWriter2 = new StreamWriter(path))
			{
				streamWriter2.Write(data, Encoding.UTF8);
			}
		}

		private void DrvsExport_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog fbd = new FolderBrowserDialog())
			{
				fbd.Description = "Выберите папку, которую необходимо использовать, или создайте новую:"; 
				if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
				{
					List<string> list = Directory.GetFiles(fbd.SelectedPath).ToList<string>();
					List<string> findedFiles = new List<string>();
					list.ForEach(delegate(string x)
					{
						if (x.EndsWith("Список установленных программ.htm") | x.EndsWith("Installed programs.htm") | x.EndsWith("Liste der installierten Programme.htm") | x.EndsWith("Список встановлених програм.htm") | x.EndsWith("Custom Windows Settings.reg"))
						{
							if (x.EndsWith(".reg"))
							{
								findedFiles.Add(x);
							}
							if (x.EndsWith(".htm"))
							{
								string text = File.ReadAllText(x);
								if (text.StartsWith("<html><head><meta") | text.EndsWith("<br></div><br><br><br></body></html>"))
								{
									findedFiles.Add(x);
								}
							}
						}
					});
					if (findedFiles.LongCount<string>() == 2L)
					{
						List<string> list2 = Directory.GetDirectories(fbd.SelectedPath).ToList<string>();
						if (!Directory.Exists(this.desktopPath + "\\Drivers for portable"))
						{
							Directory.CreateDirectory(this.desktopPath + "\\Drivers for portable");
						}
						Process process = new Process();
						process.StartInfo.FileName = "cmd";
						process.StartInfo.Arguments = "/C pnputil /export-driver * \"" + this.desktopPath + "\\Drivers for portable\"";
						process.StartInfo.CreateNoWindow = true;
						process.StartInfo.UseShellExecute = false;
						process.Start();
						process.WaitForExit();
						list2.ForEach(delegate(string x)
						{
							x = x.Replace(fbd.SelectedPath, this.desktopPath + "\\Drivers for portable");
							if (Directory.Exists(x))
							{
								Directory.Delete(x, true);
							}
						});
						MessageBox.Show("Папка с драйверами на рабочем столе", "Готово!", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					else
					{
						MessageBox.Show("Папка драйверов Win 10 Tweaker имеет неверный формат.\r\nПопробуйте создать новый экспорт и выбрать его.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private string appPath = AppDomain.CurrentDomain.BaseDirectory;
		private string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		private string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		private string exePathDone = "";
		private string sourceCode = "";
	}
}
