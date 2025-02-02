using ImageMagick;
using NLua;

namespace minitool6
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string workPath = args[0];
            string tablePath = Path.Combine(workPath, "system", "table", "list_windows.tbl");
            string cgPath = Path.Combine(workPath, "image", "cg");
            string outDirPath = Path.Combine(workPath, "output");
            if (!Directory.Exists(outDirPath))
                Directory.CreateDirectory(outDirPath);

            List<CgModeEntry> Cgs = [];

            using (var lua = new Lua())
            {
                lua.DoFile(tablePath);
                var csv = lua["csv"] as LuaTable;
                var extraCgMode = csv?["extra_cgmode"] as LuaTable;
                if (extraCgMode != null)
                {
                    foreach (var key in extraCgMode.Keys)
                    {
                        var cgMode = extraCgMode[key] as LuaTable;
                        if (cgMode != null)
                        {
                            int id = Convert.ToInt32(cgMode[2]);
                            List<string> items = [];
                            for (int i = 3; i <= cgMode.Keys.Count; i++)
                            {
                                items.Add(cgMode[i]?.ToString());
                            }
                            Cgs.Add(new CgModeEntry(id, (string)key, items));
                        }
                    }
                }
                else
                {
                    Console.WriteLine("extra_cgmode table not found!");
                }
            }

            foreach (var cg in Cgs)
            {
                foreach (string iptName in cg.Items)
                {
                    string tmpWorkingPath = Path.Combine(cgPath, cg.Name);
                    string iptPath = Path.Combine(tmpWorkingPath, iptName + ".ipt");
                    if (!File.Exists(iptPath))
                    {
                        Console.WriteLine("ipt file not found: " + iptPath);
                        continue;
                    }
                    using (var lua = new Lua())
                    {
                        lua.DoFile(iptPath);
                        var ipt = lua["ipt"] as LuaTable;
                        if (ipt != null)
                        {
                            List<CoverImg> cis = [];
                            for (int i = 1; i <= ipt.Keys.Count - 2; i++)
                            {
                                cis.Add(new CoverImg((LuaTable)ipt[i]));
                            }
                            IptEntry ipte = new IptEntry((string)ipt["mode"], new BaseImg((LuaTable)ipt["base"]), cis);
                            ProcessIpt(outDirPath, tmpWorkingPath, ipte);

                        }
                        else
                            Console.WriteLine("ipt table not found!");
                    }
                }
            }
        }

        public static void ProcessIpt(string outDirPath, string tmpWorkingPath, IptEntry ipte)
        {
            if (ipte.mode != "diff")
            {
                Console.WriteLine("mode not supported: " + ipte.mode);
                return;
            }
            string baseFilePath = Path.Combine(tmpWorkingPath, ipte.bi.file + ".png");
            string targetPath = Path.Combine(outDirPath, ipte.bi.file);
            using (var baseImage = new MagickImage(baseFilePath))
            {
                foreach (var ci in ipte.cis)
                {
                    using (var coverImage = new MagickImage(Path.Combine(tmpWorkingPath, ci.file + ".png")))
                    {
                        baseImage.Composite(coverImage, ci.x, ci.y, CompositeOperator.Over);
                    }
                    targetPath += $"_{ci.file}";
                }
                baseImage.Write(targetPath + ".png");
            }
        }
    }
}

public class CgModeEntry(int id, string name, List<string> items)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public List<string> Items { get; set; } = items;
}

public class IptEntry(string mode, BaseImg bi, List<CoverImg> cis)
{
    public string mode = mode;
    public BaseImg bi = bi;
    public List<CoverImg> cis = cis;

}
public class BaseImg(LuaTable ipt_base)
{
    public string file = ipt_base[1].ToString();
    public int w = Convert.ToInt32(ipt_base["w"]);
    public int h = Convert.ToInt32(ipt_base["h"]);
}
public class CoverImg(LuaTable ipt_cover)
{
    public int id = Convert.ToInt32(ipt_cover["id"]);
    public string file = ipt_cover["file"].ToString();
    public int x = Convert.ToInt32(ipt_cover["x"]);
    public int y = Convert.ToInt32(ipt_cover["y"]);
}

