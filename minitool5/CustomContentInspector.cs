using MimeDetective.Storage;
using MimeDetective;

namespace minitool5
{
    public class CustomContentInspector
    {
        public IContentInspector Instance { get; }

        public CustomContentInspector()
        {
            var MyDefinitions = new List<Definition>
            {
                new()
                {
                    File = new()
                    {
                        Categories = [Category.Script],
                        Description = "Kirikiri/KAG TJS script",
                        Extensions = ["TJS"],
                        MimeType = "application/x-tjs",
                    },
                    Signature = new Segment[] {
                        PrefixSegment.Create(0, "54 4a 53") // TJS
                    }.ToSignature(),
                },
                new()
                {
                    File = new()
                    {
                        Categories = [Category.Image],
                        Description = "Kirikiri/KAG TLG image",
                        Extensions = ["TLG"],
                        MimeType = "image/x-tlg",
                    },
                    Signature = new Segment[]
                    {
                        PrefixSegment.Create(0, "54 4c 47") // TLG
                    }.ToSignature(),
                },
                new()
                {
                    File = new()
                    {
                        Categories = [Category.Other],
                        Description = "M2 E-mote PSB",
                        Extensions = ["PSB"],
                        MimeType = "application/x-psb",
                    },
                    Signature = new Segment[]
                    {
                        PrefixSegment.Create(0, "50 53 42 00") // PSB\0
                    }.ToSignature(),
                },
                                new()
                {
                    File = new()
                    {
                        Categories = [Category.Other],
                        Description = "Live2D",
                        Extensions = ["L2D"],
                        MimeType = "application/x-l2d",
                    },
                    Signature = new Segment[]
                    {
                        PrefixSegment.Create(0, "50 4B 03 04 14 00 08 08") // PK...
                    }.ToSignature(),
                },
                //new() // 不可靠
                //{
                //    File = new()
                //    {
                //        Categories = [Category.Script],
                //        Description = "Kirikiri/KAG KS script",
                //        Extensions = ["KS"],
                //        MimeType = "application/x-ks",
                //    },
                //    Signature = new Segment[]
                //    {
                //        PrefixSegment.Create(0, "ff fe") // 
                //    }.ToSignature(),
                //},
            };

            Instance = new ContentInspectorBuilder()
            {
                Definitions = MyDefinitions,
                StringSegmentOptions = new()
                {
                    OptimizeFor = MimeDetective.Engine.StringSegmentResourceOptimization.HighSpeed,
                },
            }.Build();
        }

    }
}
