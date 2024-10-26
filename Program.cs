using LibGit2Sharp;
using System.Diagnostics;

bool repair = false;
string workdir = string.Empty;

for (int i = 0; i < args.Length; i++)
{
	if (args[i] == "-repair")
		repair = true;
	else if (args[i] == "-workdir")
		workdir = args[++i];
}

if (workdir == string.Empty)
{
	workdir = System.IO.Directory.GetCurrentDirectory();
	var temp = Path.GetTempPath();
	File.Copy(workdir + "\\NSCN_Updater.exe", temp + "\\NSCN_Updater.exe", true);

	var files = Directory.GetFiles(workdir, "*git*.dll");

	foreach (var file in files)
	{
		var filename = Path.GetFileName(file);
		File.Copy(file, temp + "\\" + filename, true);
	}
	string sub_args = "-workdir " + workdir;
	if (repair)
		sub_args += " -repair";
	Process.Start(temp + "\\NSCN_Updater.exe", sub_args);
	Environment.Exit(0);
}

var path = workdir;
var url = "https://gitee.com/st0n1e/NSCN_Launcher.git";

if (!Repository.IsValid(path))
{
	if (!File.Exists(workdir + "\\Titanfall2.exe"))
	{
		Console.WriteLine("未找到Titanfall2.exe");
		Console.WriteLine("请确认是否已将本程序放置在了正确的目录下");
		Environment.Exit(1);
	}
	Repository.Init(path);
}

using (var repo = new Repository(path))
{
	if (repair)
	{
		repo.Reset(ResetMode.Hard, @"origin/master");
		Console.WriteLine("修复完毕！");
		Thread.Sleep(2000);
		Environment.Exit(0);
	}
	var options = new FetchOptions();

	options.Prune = true;
	options.TagFetchMode = TagFetchMode.Auto;

	if (repo.Network.Remotes["origin"] != null)
	{
		Console.WriteLine("已经设置remote，更新remote ...");
		repo.Network.Remotes.Update("origin", r => r.Url = url);
	}
	else
	{
		Console.WriteLine("未设置remote，设置remote ...");
		repo.Network.Remotes.Add("origin", url);
	}

	var old_commit = repo.Commits.Any() ? repo.Commits.First().ToString() : null;
	Console.WriteLine("目前的commit: " + old_commit);

	var remote = repo.Network.Remotes["origin"];
	var msg = "Fetching remote";
	var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);

	Console.WriteLine("正在下载更新 ...");
	Commands.Fetch(repo, remote.Name, refSpecs, options, msg);

	var new_commit = repo.Branches["origin/master"].Commits.First().ToString();
	Console.WriteLine("最新的commit: " + new_commit);

	if (new_commit != old_commit)
	{
		Console.WriteLine("正在更新文件 ...");
		repo.Reset(ResetMode.Hard, @"origin/master");
		Process.Start(workdir + "\\NSCN_Updater.exe");
		Environment.Exit(0);
	}
	else
	{
		Console.WriteLine("今日无事可做 ...");
	}
}

if (!File.Exists(workdir + "\\R2Northstar\\mods\\Northstar.CustomServers\\mod\\cfg\\autoexec_ns_server.cfg"))
{
	File.Copy(workdir + "\\persist\\autoexec_ns_server.cfg", workdir + "\\R2Northstar\\mods\\Northstar.CustomServers\\mod\\cfg\\autoexec_ns_server.cfg");
}
if (!File.Exists(workdir + "\\ns_startup_args_dedi.txt"))
{
	File.Copy(workdir + "\\persist\\ns_startup_args_dedi.txt", workdir + "\\ns_startup_args_dedi.txt");
}
if (!File.Exists(workdir + "\\ns_startup_args_dedi.txt"))
{
	File.Copy(workdir + "\\persist\\ns_startup_args_dedi.txt", workdir + "\\ns_startup_args_dedi.txt");
}

Process.Start(workdir + "\\NorthstarLauncher.exe");
