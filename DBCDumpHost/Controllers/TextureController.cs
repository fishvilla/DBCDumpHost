﻿using DBCDumpHost.Services;
using DBCDumpHost.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DBCDumpHost.Controllers
{
    [Route("api/itemtexture")]
    [Route("api/texture")]
    [ApiController]
    public class TextureController : ControllerBase
    {
        private readonly DBCManager dbcManager;

        public TextureController(IDBCManager dbcManager)
        {
            this.dbcManager = dbcManager as DBCManager;
        }


        // GET: data/
        [HttpGet]
        public string Get()
        {
            return "No filedataid given!";
        }

        // GET: data/name
        [HttpGet("{filedataid}")]
        public Dictionary<uint, List<uint>> Get(int filedataid, string build)
        {
            Logger.WriteLine("Serving texture lookup for filedataid " + filedataid + " build " + build);

            var modelFileData = dbcManager.GetOrLoad("modelfiledata", build);
            var itemDisplayInfo = dbcManager.GetOrLoad("itemdisplayinfo", build);
            var textureFileData = dbcManager.GetOrLoad("texturefiledata", build);
            var creatureModelData = dbcManager.GetOrLoad("creaturemodeldata", build);
            var creatureDisplayInfo = dbcManager.GetOrLoad("creaturedisplayinfo", build);

            var returnList = new Dictionary<uint, List<uint>>();

            if (modelFileData.ContainsKey(filedataid))
            {
                dynamic mfdEntry = modelFileData[filedataid];

                foreach (dynamic idiEntry in itemDisplayInfo.Values)
                {
                    if (idiEntry.ModelResourcesID[0] != mfdEntry.ModelResourcesID && idiEntry.ModelResourcesID[1] != mfdEntry.ModelResourcesID)
                    {
                        continue;
                    }

                    var textureFileDataList = new List<uint>();

                    foreach (dynamic tfdEntry in textureFileData.Values)
                    {
                        if (tfdEntry.MaterialResourcesID == idiEntry.ModelMaterialResourcesID[0] || tfdEntry.MaterialResourcesID == idiEntry.ModelMaterialResourcesID[1])
                        {
                            textureFileDataList.Add((uint)tfdEntry.FileDataID);
                        }
                    }

                    returnList.Add((uint)idiEntry.ID, textureFileDataList);
                }

                foreach (dynamic cmdEntry in creatureModelData.Values)
                {
                    if (cmdEntry.FileDataID != filedataid)
                    {
                        continue;
                    }

                    foreach (dynamic cdiEntry in creatureDisplayInfo.Values)
                    {
                        if (cdiEntry.ModelID != cmdEntry.ID)
                        {
                            continue;
                        }

                        returnList.Add((uint)cdiEntry.ID, new List<uint> { (uint)cdiEntry.TextureVariationFileDataID[0], (uint)cdiEntry.TextureVariationFileDataID[1], (uint)cdiEntry.TextureVariationFileDataID[2] });
                    }

                    break;
                }
            }

            return returnList;
        }
    }
}
