using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Diagrams;

namespace FlowChartDemo1.Controllers
{


    
    
    public class FlowChartController : Controller
    {


        [HttpPost]
        public ActionResult FlowChart(HttpPostedFileBase file)
        {

            var nodes = TempData["nodes"];

            var connectors = TempData["connectors"];

            List<SymbolPalettePalette> palettes = (List<SymbolPalettePalette>)TempData["Palette"];
            
            var gridLines = TempData["gridLines"];
            
            var margin = TempData["margin"];
            if(palettes.Count < 3) {
                List<DiagramNode> Images = new List<DiagramNode>();

                if (file != null)
                {

                    for (int i = 0; i < Request.Files.Count; i++)
                    {

                        string fileLocation = Server.MapPath("~/Content/") + Request.Files[i].FileName;
                        if (System.IO.File.Exists(fileLocation))
                        {
                            System.IO.File.Delete(fileLocation);
                        }
                        Request.Files[i].SaveAs(fileLocation);
                        DiagramNode node = new DiagramNode()
                        {
                            Id = Request.Files[i].FileName,
                            Width = 100,
                            Height = 100,
                            OffsetX = 100,
                            OffsetY = 100,
                            Shape = new { type = "Image", source = "/Content/" + Request.Files[i].FileName },
                            Style = new NodeStyleNodes() { Fill = "None" }
                        };

                        Images.Add(node);
                    }


                }

                palettes.Insert(0,new SymbolPalettePalette() { Id = "images", Expanded = true, Symbols = Images, IconCss = "shapes", Title = "Custom Images" });
            }
            else
            {
                List<DiagramNode> Images = (List<DiagramNode>)palettes[0].Symbols;

                if (file != null)
                {
                    List<string> ids = new List<string>();
                    foreach(var x in Images)
                    {
                        string y = (string)x.Id;
                        ids.Add(y);
                    }
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        string name = Request.Files[i].FileName;
                        if (!ids.Contains(name))
                        {
                            string fileLocation = Server.MapPath("~/Content/") + Request.Files[i].FileName;
                            if (System.IO.File.Exists(fileLocation))
                            {
                                System.IO.File.Delete(fileLocation);
                            }
                            Request.Files[i].SaveAs(fileLocation);
                            DiagramNode node = new DiagramNode()
                            {
                                Id = Request.Files[i].FileName,
                                Width = 100,
                                Height = 100,
                                OffsetX = 100,
                                OffsetY = 100,
                                Shape = new { type = "Image", source = "/Content/" + Request.Files[i].FileName },
                                Style = new NodeStyleNodes() { Fill = "None" }
                            };
                            ids.Add(Request.Files[i].FileName);
                            Images.Add(node);
                        }
                    }

                    palettes.RemoveAt(0);
                    palettes.Insert(0, new SymbolPalettePalette() { Id = "images", Expanded = true, Symbols = Images, IconCss = "shapes", Title = "Custom Images" });

                }


            }

            ViewBag.nodes = nodes;
            TempData["nodes"] = ViewBag.nodes;
            ViewBag.connectors = connectors;
            TempData["connectors"] = ViewBag.connectors;
            ViewBag.Palette = palettes;
            TempData["Palette"] = palettes;
            ViewBag.gridLines = gridLines;
            TempData["gridLines"] = ViewBag.gridLines;
            ViewBag.margin = margin;
            TempData["margin"] = ViewBag.margin;

            return View();
        }

        // GET: Serialization
        public ActionResult FlowChart() 
        {
            List<DiagramNode> nodes = new List<DiagramNode>();
            nodes.Add(new DiagramNode()
            {
                Id = "Start",
                OffsetX = 300,
                OffsetY = 80,
                Width = 100,
                Height = 50,
                Style = new DiagramShapeStyle() { Fill = "#d0f0f1", StrokeColor = "#797979" },
                Annotations = new List<DiagramNodeAnnotation>() {
                new DiagramNodeAnnotation() { Content = "Place Order" }
                },
                Shape = new { type = "Flow", shape = "Terminator" }
            });
            nodes.Add(new DiagramNode()
            {
                Id = "Alarm",
                OffsetX = 300,
                OffsetY = 160,
                Width = 100,
                Height = 50,
                Style = new DiagramShapeStyle() { Fill = "#fbfdc5", StrokeColor = "#797979" },
                Annotations = new List<DiagramNodeAnnotation>() {
                new DiagramNodeAnnotation() { Content = "Start Transaction" }},
                Shape = new { type = "Flow", shape = "Process" }
            });
            nodes.Add(new DiagramNode()
            {
                Id = "Ready",
                OffsetX = 300,
                OffsetY = 240,
                Width = 100,
                Height = 50,
                Style = new DiagramShapeStyle() { Fill = "#ffcfd2", StrokeColor = "#797979" },
                Annotations = new List<DiagramNodeAnnotation>() {
                    new DiagramNodeAnnotation() { Content = "Credit Card ValId?" }
                },
                Shape = new { type = "Flow", shape = "Decision" }
            });
            nodes.Add(new DiagramNode()
            {
                Id = "Climb",
                OffsetX = 300,
                OffsetY = 330,
                Width = 100,
                Height = 50,
                Style = new DiagramShapeStyle() { Fill = "#ffcfd2", StrokeColor = "#797979" },
                Annotations = new List<DiagramNodeAnnotation>() {
                new DiagramNodeAnnotation() { Content = "Have Funds?" }},
                Shape = new { type = "Flow", shape = "Decision" }
            });
            nodes.Add(new DiagramNode()
            {
                Id = "End",
                OffsetX = 300,
                OffsetY = 430,
                Width = 100,
                Height = 50,
                Style = new DiagramShapeStyle() { Fill = "#d0f0f1", StrokeColor = "#797979" },
                Annotations = new List<DiagramNodeAnnotation>() {
                new DiagramNodeAnnotation() { Content = "Order Placed" }},
                Shape = new { type = "Flow", shape = "Terminator" }
            });
            nodes.Add(new DiagramNode()
            {
                Id = "Relay",
                OffsetX = 500,
                OffsetY = 160,
                Width = 100,
                Height = 50,
                Style = new DiagramShapeStyle() { Fill = "#eaf4f4", StrokeColor = "#797979" },
                Annotations = new List<DiagramNodeAnnotation>() {
                new DiagramNodeAnnotation() { Content = "Relay" }},
                Shape = new { type = "Flow", shape = "Delay" }
            });
            nodes.Add(new DiagramNode()
            {
                Id = "Hit",
                OffsetX = 500,
                OffsetY = 240,
                Width = 100,
                Height = 50,
                Style = new DiagramShapeStyle() { Fill = "#fbfdc5", StrokeColor = "#797979" },
                Annotations = new List<DiagramNodeAnnotation>() {
                new DiagramNodeAnnotation() { Content = "Restart Transaction", Margin = new DiagramMargin() { Left = 10, Right = 10, Bottom = 10, Top = 10 } }},
                Shape = new { type = "Flow", shape = "Process" }
            });

            List<DiagramConnector> connectors = new List<DiagramConnector>();
            connectors.Add(new DiagramConnector() { Id = "connector1", SourceID = "Start", TargetID = "Alarm" });
            
            connectors.Add(new DiagramConnector() { Id = "connector2", SourceID = "Alarm", TargetID = "Ready" });
            
            List<DiagramConnectorAnnotation> Connector3 = new List<DiagramConnectorAnnotation>();
            Connector3.Add(new DiagramConnectorAnnotation() { Content = "Yes", Style = new DiagramTextStyle() { Fill = "white" } });
            connectors.Add(new DiagramConnector() { Id = "connector3", SourceID = "Ready", TargetID = "Climb", Annotations=Connector3 });
            
            List<DiagramConnectorAnnotation> Connector4 = new List<DiagramConnectorAnnotation>();
            Connector4.Add(new DiagramConnectorAnnotation() { Content = "Yes", Style = new DiagramTextStyle() { Fill = "white" } });
            connectors.Add(new DiagramConnector() { Id = "connector4", SourceID = "Climb", TargetID = "End", Annotations=Connector4 });
            
            List<DiagramConnectorAnnotation> Connector5 = new List<DiagramConnectorAnnotation>();
            Connector5.Add(new DiagramConnectorAnnotation() { Content = "No", Style = new DiagramTextStyle() { Fill = "white" } });
            connectors.Add(new DiagramConnector() { Id = "connector5", SourceID = "Ready", TargetID = "Hit", Annotations = Connector5 }) ;
            
            connectors.Add(new DiagramConnector() { Id = "connector6", SourceID = "Hit", TargetID = "Relay" });
            
            connectors.Add(new DiagramConnector() { Id = "connector7", SourceID = "Relay", TargetID = "Alarm" });
            
            List<DiagramConnectorAnnotation> Connector8 = new List<DiagramConnectorAnnotation>();
            Connector8.Add(new DiagramConnectorAnnotation() { Content = "No", Style = new DiagramTextStyle() { Fill = "white" } });
            connectors.Add(new DiagramConnector() { Id = "connector8", SourceID = "Climb", TargetID = "Hit", Annotations=Connector8, Type = Segments.Orthogonal });
            
            

            List<DiagramNode> flowShapes = new List<DiagramNode>();
            
            flowShapes.Add(new DiagramNode() { Id = "Terminator", Shape = new { type = "Flow", shape = "Terminator" }, Style = new NodeStyleNodes() { Fill = "#d0f0f1" } });
            flowShapes.Add(new DiagramNode() { Id = "Process", Shape = new { type = "Flow", shape = "Process" }, Style = new NodeStyleNodes() { Fill = "#fbf8cc" } });
            flowShapes.Add(new DiagramNode() { Id = "Decision", Shape = new { type = "Flow", shape = "Decision" }, Style = new NodeStyleNodes() { Fill = "#ffcfd2" } });
            flowShapes.Add(new DiagramNode() { Id = "Document", Shape = new { type = "Flow", shape = "Document" }, Style = new NodeStyleNodes() { Fill = "#f1c0e8" } });
            flowShapes.Add(new DiagramNode() { Id = "PreDefinedProcess", Shape = new { type = "Flow", shape = "PreDefinedProcess" }, Style = new NodeStyleNodes() { Fill = "#cfbaf0" } });
            flowShapes.Add(new DiagramNode() { Id = "PaperTap", Shape = new { type = "Flow", shape = "PaperTap" } , Style = new NodeStyleNodes() { Fill = "#a3c4f3" } });
            flowShapes.Add(new DiagramNode() { Id = "DirectData", Shape = new { type = "Flow", shape = "DirectData" } , Style = new NodeStyleNodes() { Fill = "#90dbf4" } });
            flowShapes.Add(new DiagramNode() { Id = "SequentialData", Shape = new { type = "Flow", shape = "SequentialData" } , Style = new NodeStyleNodes() { Fill = "#8eecf5" } });
            flowShapes.Add(new DiagramNode() { Id = "Sort", Shape = new { type = "Flow", shape = "Sort" } , Style = new NodeStyleNodes() { Fill = "#98f5e1" } });
            flowShapes.Add(new DiagramNode() { Id = "MultiDocument", Shape = new { type = "Flow", shape = "MultiDocument" } , Style = new NodeStyleNodes() { Fill = "#b9fbc0" } });
            flowShapes.Add(new DiagramNode() { Id = "Collate", Shape = new { type = "Flow", shape = "Collate" } , Style = new NodeStyleNodes() { Fill = "#809bce" } });
            flowShapes.Add(new DiagramNode() { Id = "SummingJunction", Shape = new { type = "Flow", shape = "SummingJunction" }  , Style = new NodeStyleNodes() { Fill = "#95b8d1" } });
            flowShapes.Add(new DiagramNode() { Id = "Or", Shape = new { type = "Flow", shape = "Or" } , Style = new NodeStyleNodes() { Fill = "#b8e0d2" } });
            flowShapes.Add(new DiagramNode() { Id = "InternalStorage", Shape = new { type = "Flow", shape = "InternalStorage" } , Style = new NodeStyleNodes() { Fill = "#d6eadf" } });
            flowShapes.Add(new DiagramNode() { Id = "Extract", Shape = new { type = "Flow", shape = "Extract" } , Style = new NodeStyleNodes() { Fill = "#eac4d5" } });
            flowShapes.Add(new DiagramNode() { Id = "ManualOperation", Shape = new { type = "Flow", shape = "ManualOperation" } , Style = new NodeStyleNodes() { Fill = "#b2967d" } });
            flowShapes.Add(new DiagramNode() { Id = "Merge", Shape = new { type = "Flow", shape = "Merge" } , Style = new NodeStyleNodes() { Fill = "#e6beae" } });
            flowShapes.Add(new DiagramNode() { Id = "OffPageReference", Shape = new { type = "Flow", shape = "OffPageReference" } , Style = new NodeStyleNodes() { Fill = "#e7d8c9" } });
            flowShapes.Add(new DiagramNode() { Id = "SequentialAccessStorage", Shape = new { type = "Flow", shape = "SequentialAccessStorage" } , Style = new NodeStyleNodes() { Fill = "#eee4e1" } });
            flowShapes.Add(new DiagramNode() { Id = "Annotation", Shape = new { type = "Flow", shape = "Annotation" }, Style = new NodeStyleNodes() { Fill = "#ecf8f8" } });
            flowShapes.Add(new DiagramNode() { Id = "Annotation2", Shape = new { type = "Flow", shape = "Annotation2" }, Style = new NodeStyleNodes() { Fill = "#6b9080" } });
            flowShapes.Add(new DiagramNode() { Id = "Data", Shape = new { type = "Flow", shape = "Data" }, Style = new NodeStyleNodes() { Fill = "#a4c3b2" } });
            flowShapes.Add(new DiagramNode() { Id = "Card", Shape = new { type = "Flow", shape = "Card" }, Style = new NodeStyleNodes() { Fill = "#cce3de" } });
            flowShapes.Add(new DiagramNode() { Id = "Delay", Shape = new { type = "Flow", shape = "Delay" }, Style = new NodeStyleNodes() { Fill = "#eaf4f4" } });
            flowShapes.Add(new DiagramNode() { Id = "Hex", Shape = new { type = "Basic", shape = "Hexagon" }, Style = new NodeStyleNodes() { Fill = "#f6fff8" } });
            
            
            
            flowShapes.Add(new DiagramNode()
            {
                Id = "node1",
                Width = 100,
                Height = 100,
                OffsetX = 100,
                OffsetY = 100,
                Shape = new
                {
                    type = "Path",
                    data = "M 3 8 L 10 1 L 13 0 L 12 3 C 4 9 6 11 7 11 C 7 11 8 12 7 12 A 1.42 1.42 0 0 1 6 13 A 5 5 0 0 0 4 10 Q 3.5 9.9 3.5 10.5 T 2 11.8 T 1.2 11 T 2.5 9.5 T 3 9 A 5 5 90 0 0 0 7 A 1.42 1.42 0 0 1 1 6 C 1 5 2 6 3 6 C 2 7 3 8 6 5 M 10 1 L 10 3 L 12 3 L 10.2 2.8 L 10 1"
                },
                Style= new NodeStyleNodes() { Fill="red"}
            });

            


            List<DiagramConnector> paletteConnectors = new List<DiagramConnector>();
            paletteConnectors.Add(new DiagramConnector()
            {
                Id = "Link1",
                Type = Segments.Orthogonal,
                SourcePoint = new DiagramPoint() { X = 0, Y = 0 },
                TargetPoint = new DiagramPoint() { X = 40, Y = 40 },
                TargetDecorator = new ConnectorTargetDecoratorConnectors() { Shape = DecoratorShapes.Arrow, Style = new DiagramShapeStyle() { StrokeColor = "#757575", Fill = "#757575" } },
                Style = new DiagramStrokeStyle() { StrokeWidth = 2, StrokeColor = "#757575" }
            });
            paletteConnectors.Add(new DiagramConnector()
            {
                Id = "Link2",
                Type = Segments.Orthogonal,
                SourcePoint = new DiagramPoint() { X = 0, Y = 0 },
                TargetPoint = new DiagramPoint() { X = 40, Y = 40 },
                TargetDecorator = new ConnectorTargetDecoratorConnectors() { Shape = DecoratorShapes.None },
                Style = new DiagramStrokeStyle() { StrokeWidth = 2, StrokeColor = "#757575" }
            });
            paletteConnectors.Add(new DiagramConnector()
            {
                Id = "Link3",
                Type = Segments.Straight,
                SourcePoint = new DiagramPoint() { X = 0, Y = 0 },
                TargetPoint = new DiagramPoint() { X = 40, Y = 40 },
                TargetDecorator = new ConnectorTargetDecoratorConnectors() { Shape = DecoratorShapes.Arrow, Style = new DiagramShapeStyle() { StrokeColor = "#757575", Fill = "#757575" } },
                Style = new DiagramStrokeStyle() { StrokeWidth = 2, StrokeColor = "#757575" }
            });
            paletteConnectors.Add(new DiagramConnector()
            {
                Id = "Link4",
                Type = Segments.Straight,
                SourcePoint = new DiagramPoint() { X = 0, Y = 0 },
                TargetPoint = new DiagramPoint() { X = 40, Y = 40 },
                TargetDecorator = new ConnectorTargetDecoratorConnectors() { Shape = DecoratorShapes.None },
                Style = new DiagramStrokeStyle() { StrokeWidth = 2, StrokeColor = "#757575" }
            });
            paletteConnectors.Add(new DiagramConnector()
            {
                Id = "Link5",
                Type = Segments.Bezier,
                SourcePoint = new DiagramPoint() { X = 0, Y = 0 },
                TargetPoint = new DiagramPoint() { X = 40, Y = 40 },
                TargetDecorator = new ConnectorTargetDecoratorConnectors() { Shape = DecoratorShapes.None },
                Style = new DiagramStrokeStyle() { StrokeWidth = 2, StrokeColor = "#757575" }
            });

            double[] intervals = { 1, 9, 0.25, 9.75, 0.25, 9.75, 0.25, 9.75, 0.25, 9.75, 0.25, 9.75, 0.25, 9.75, 0.25, 9.75, 0.25, 9.75, 0.25, 9.75 };
            DiagramGridlines grIdLines = new DiagramGridlines()
            { LineColor = "#e0e0e0", LineIntervals = intervals };
            
            DiagramMargin margin = new DiagramMargin() { Left = 15, Right = 15, Bottom = 15, Top = 15 };
            

            List<SymbolPalettePalette> palettes = new List<SymbolPalettePalette>();
            palettes.Add(new SymbolPalettePalette() { Id = "flow", Expanded = true, Symbols = flowShapes, IconCss = "shapes", Title = "Flow Shapes" });
            palettes.Add(new SymbolPalettePalette() { Id = "connectors", Expanded = true, Symbols = paletteConnectors, IconCss = "shapes", Title = "Connectors" });

            ViewBag.nodes = nodes;
            TempData["nodes"] = ViewBag.nodes;
            ViewBag.connectors = connectors;
            TempData["connectors"] = ViewBag.connectors;
            ViewBag.Palette = palettes;
            TempData["Palette"] = palettes;
            ViewBag.gridLines = grIdLines;
            TempData["gridLines"] = ViewBag.gridLines;
            ViewBag.margin = margin;
            TempData["margin"] = ViewBag.margin;
            //ViewBag.Spconnectors = paletteConnectors;


            List<contextMenuItems> item = new List<contextMenuItems>();
            item.Add(new contextMenuItems()
            {
                Id = "openTask",
                Text = "Open Task",
                Target = ".e-elementcontent",
                IconCss = "e-save"
            });
            item.Add(new contextMenuItems()
            { 
                Id = "assginTask",
                Text = "Assign Task ID",
                Target = ".e-elementcontent",
                IconCss = "e-save"
            });
            

            ViewBag.item = item;

            return View();
        }
        
        
    }
    public class contextMenuItems
    {
        [DefaultValue(null)]
        [HtmlAttributeName("text")]
        [JsonProperty("text")]
        public string Text
        {
            get;
            set;
        }
        [DefaultValue(null)]
        [HtmlAttributeName("id")]
        [JsonProperty("id")]
        public string Id
        {
            get;
            set;
        }
        [DefaultValue(null)]
        [HtmlAttributeName("target")]
        [JsonProperty("target")]
        public string Target
        {
            get;
            set;
        }
        [DefaultValue(null)]
        [HtmlAttributeName("iconCss")]
        [JsonProperty("iconCss")]
        public string IconCss
        {
            get;
            set;
        }
    }
}