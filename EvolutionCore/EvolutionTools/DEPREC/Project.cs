using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using System.Windows.Forms;

namespace EvolutionTools
{
    public class Project
    {
        //Fields
        protected bool _needSave = false;
        protected string _filePath;

        //Setters
        public void SetNeedSave()
        {
            this._needSave = true;
        }

        //Properties
        public string FilePath
        {
            get
            {
                return this._filePath;
            }
        }

        //Constructor
        public Project()
        {
        }

        //Methods
        public void Save(string fullpath, bool overWrite)
        {
            //Check for files
            if (File.Exists(fullpath))
            {
                if (overWrite)
                    File.Delete(fullpath);
                else
                    throw new NotImplementedException();
            }

            //Write File
            Management.WriteTextToFile(fullpath, new string[1] { "a" }, false);

            //Set Project to current path
            this._filePath = fullpath;
        }
    }
}
