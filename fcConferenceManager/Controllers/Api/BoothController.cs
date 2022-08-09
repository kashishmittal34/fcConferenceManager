using MAGI_API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static MAGI_API.Models.BoothValidate;
using Action = MAGI_API.Models.BoothValidate.Action;

namespace fcConferenceManager.Controllers
{
    [RoutePrefix("api/Booth")]
    public class BoothController : ApiController
    {
        static BoothOperation repository = new BoothOperation();
        [HttpGet]
        [Route("Scene/{Event_pkey}")]
        public async Task<MainBooth> Scene(string Event_pkey)
        {
            MainBooth mBooth = await PrepareList(Event_pkey);
            return mBooth;
        }

        [HttpPost]
        [Route("BoothList/{Event_pkey}")]
        public async Task<MainBooth> BoothList(string Event_pkey)
        {
            MainBooth mBooth = await PrepareList(Event_pkey);
            string syncFile = HttpContext.Current.Server.MapPath("~/Temp3D/settings/scene.json");
            string data = JsonConvert.SerializeObject(mBooth);
            System.IO.File.WriteAllText(syncFile, data);
            return mBooth;
        }

        private async Task<MainBooth> PrepareList(string Event_pkey)
        {
            MainBooth mBooth = new MainBooth();
            List<BoothList> li = await repository.Booth_item_select(Event_pkey);            
            List<Booth> mi = new List<Booth>();
            if (li.Count > 0)
            {
                foreach (BoothList item in li)
                {
                    Booth booth = new Booth();
                    booth.boothName = "Booth" + item.pkey.ToString().PadLeft(3, '0');
                    booth.exhibitorName = item.SettingName;
                    booth.package = "BluePackage";
                    booth.id = item.pkey.ToString();
                    if (!string.IsNullOrEmpty(item.BoothColor))
                    {
                        Match m = Regex.Match(item.BoothColor, @"A=(?<Alpha>\d+),\s*R=(?<Red>\d+),\s*G=(?<Green>\d+),\s*B=(?<Blue>\d+)");
                        if (m.Success)
                        {
                            int alpha = int.Parse(m.Groups["Alpha"].Value);
                            int red = int.Parse(m.Groups["Red"].Value);
                            int green = int.Parse(m.Groups["Green"].Value);
                            int blue = int.Parse(m.Groups["Blue"].Value);
                            Color c = Color.FromArgb(alpha, red, green, blue);
                            string hexCode = ColorTranslator.ToHtml(c);
                            booth.color = "#" + hexCode.Replace("#", "");
                        }
                    }
                    booth.customs = new List<Custom>();
                    Custom custom = new Custom();
                    custom.name = "Sign";
                    custom.visible = true;
                    custom.texture = !string.IsNullOrEmpty(item.SignImage) ? item.SignImage : booth.boothName + ".jpg";
                    booth.customs.Add(custom);
                    custom = new Custom();
                    custom.name = "Rollup";
                    custom.visible = true;
                    custom.texture = !string.IsNullOrEmpty(item.DeskImage) ? item.DeskImage : "MAGI_empty.jpg";
                    booth.customs.Add(custom);
                    custom = new Custom();
                    custom.name = "Screen";
                    custom.visible = true;
                    custom.texture = !string.IsNullOrEmpty(item.ScreenImage) ? item.ScreenImage : "MAGI_empty.jpg";
                    booth.customs.Add(custom);
                    custom = new Custom();
                    custom.name = "Desk";
                    custom.visible = true;
                    custom.texture = !string.IsNullOrEmpty(item.SignImage) ? item.SignImage : booth.boothName + ".jpg";
                    booth.customs.Add(custom);
                    custom = new Custom();
                    custom.name = "Background";
                    custom.visible = true;
                    custom.texture = !string.IsNullOrEmpty(item.WallBackground) ? item.WallBackground : "MAGI_empty.jpg";
                    booth.customs.Add(custom);
                    custom = new Custom();
                    custom.name = "Mag1";
                    custom.visible = true;
                    custom.texture = !string.IsNullOrEmpty(item.TableRack) ? item.TableRack : "MAGI_empty.jpg";
                    booth.customs.Add(custom);
                    custom = new Custom();
                    custom.name = "Mag2";
                    custom.visible = true;
                    custom.texture = !string.IsNullOrEmpty(item.TableRack) ? item.TableRack : "MAGI_empty.jpg";
                    booth.customs.Add(custom);
                    custom = new Custom();
                    custom.name = "Mag3";
                    custom.visible = true;
                    custom.texture = !string.IsNullOrEmpty(item.TableRack) ? item.TableRack : "MAGI_empty.jpg";
                    booth.customs.Add(custom);
                    custom = new Custom();
                    custom.name = "Mag4";
                    custom.visible = true;
                    custom.texture = !string.IsNullOrEmpty(item.TableRack) ? item.TableRack : "MAGI_empty.jpg";
                    booth.customs.Add(custom);
                    custom = new Custom();
                    custom.name = "Boxes";
                    custom.visible = true;
                    custom.texture = "Boxes.jpg";
                    booth.customs.Add(custom);
                    booth.additionals = new List<Additional>();
                    if (!string.IsNullOrEmpty(item.Person))
                    {
                        Additional additional = new Additional();
                        additional.name = item.Person.Replace(".jpg", "");
                        additional.texture = null;
                        additional.model = additional.name + ".glb";
                        var array = new[] { 0, 0, 2.5 };
                        List<double> list = array.ToList();
                        additional.pos = list;
                        booth.additionals.Add(additional);
                    }
                    if (!string.IsNullOrEmpty(item.PlantID))
                    {
                        Additional additional = new Additional();                        
                        additional.texture = null;                        
                        if (item.PlantID.Contains("-2.jpg"))
                        {
                            additional.name = item.PlantID.Replace("-2.jpg", "").Replace(".jpg", "");
                            additional.model = additional.name + ".glb";
                            var array = new[] { 1.7, 0, 2.7 };
                            List<double> list = array.ToList();
                            additional.pos = list;
                        }
                        else
                        {
                            additional.name = item.PlantID.Replace("-1.jpg", "").Replace(".jpg", "");
                            additional.model = additional.name + ".glb";    
                            var array = new[] { -1.7, 0, 2.7 };
                            List<double> list = array.ToList();
                            additional.pos = list;                          
                        }
                        booth.additionals.Add(additional);
                    }
                    booth.actives = new List<Active>();
                    Active active = new Active();
                    active.name = "pointer";
                    active.texture = null;
                    active.model = null;
                    var array2 = new[] { 0, 0, 0 };
                    List<int> list2 = array2.ToList();
                    active.pos = list2;
                    active.actions = new List<Action>();
                    Action action = new Action();
                    Over over = new Over();
                    over.type = "rot";
                    over._params = new Params() { x = 0, y = "+3.14", z = 0 };
                    action.over = over;
                    Out _out = new Out();
                    _out.type = "stop";
                    action._out = _out;
                    over.type = "rot";
                    Click click = new Click();
                    click.type = "goto";
                    click._params = new Params() { targetName = booth.boothName };
                    action.click = click;
                    active.actions.Add(action);
                    booth.actives.Add(active);

                    mi.Add(booth);
                }
            }
            mBooth.booths = mi;
            mBooth.additionals = new List<Additional>();

            return mBooth;
        }

        [HttpPost]
        [Authorize(Roles = "KeyUser")]
        [Route("GetAttendeeLog/{eventKey}/{logType}/{OrgType}/{accPkey}")]
        public async Task<IHttpActionResult> GetAttendeeLog(string eventKey, string logType, string OrgType, string accPkey)
        {
            try
            {
                MyBooth booth = new MyBooth();
                return Ok(await booth.SelectAttendeeLog(eventKey, logType, OrgType, accPkey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "KeyUser")]
        [Route("GetBoothLookups")]
        public async Task<IHttpActionResult> GetBoothLookups()
        {
            try
            {
                MyBooth booth = new MyBooth();
                return Ok(await booth.GetBoothLookups());
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }


        [HttpPost]
        [Authorize(Roles = "KeyUser")]
        [Route("GetEvents/{EventPKey}/{OrganizationPKey}/{AccountPKey}")]
        public async Task<IHttpActionResult> GetEvents(string EventPKey, string OrganizationPKey, string AccountPKey)
        {
            try
            {
                MyBooth booth = new MyBooth();
                return Ok(await booth.SelectEvents(EventPKey,OrganizationPKey,AccountPKey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }


        [HttpPost]
        [Authorize(Roles = "KeyUser")]
        [Route("GetDocVideos/{OrganizationPKey}")]
        public async Task<IHttpActionResult> GetDocVideos(string OrganizationPKey)
        {
            try
            {
                MyBooth booth = new MyBooth();
                return Ok(await booth.SelectDocVideos(OrganizationPKey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "KeyUser")]
        [Route("GetStaffingSchedules/{EventOrganizationPKey}/{ActiveEventPkey}")]
        public async Task<IHttpActionResult> GetStaffingSchedules(string EventOrganizationPKey, string ActiveEventPkey)
        {
            try
            {
                MyBooth booth = new MyBooth();
                return Ok(await booth.selectStaffingSchedules(EventOrganizationPKey, ActiveEventPkey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "KeyUser")]
        [Route("GetExhibtorPersonnels/{EventOrganizationPKey}/{ActiveEventPkey}")]
        public async Task<IHttpActionResult> GetExhibtorPersonnels(string EventOrganizationPKey, string ActiveEventPkey)
        {
            try
            {
                MyBooth booth = new MyBooth();
                return Ok(await booth.selectExhibtorPersonnels(EventOrganizationPKey, ActiveEventPkey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "KeyUser")]
        [Route("SaveEvent/{EventOrganizationPKey}/{ActiveEventPkey}")]
        public async Task<IHttpActionResult> SaveEvent()
        {
            try
            {
                //MyBooth booth = new MyBooth();
                return Ok();
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }
    }
}
