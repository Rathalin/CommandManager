using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommandManager.Data
{
    public class Command : INotifyPropertyChanged
    {
        // Constructors 

        public Command() { }

        public Command(string name, string description, string script, bool showOutput)
        {
            _name = name;
            _desc = description;
            _script = script;
            _showOutput = showOutput;
        }

        /*
        public Command(int id, string name, string description, string script, bool showOutput)
            :this(name, description, script, showOutput)
        {
            ID = id;
        }
        */

        // Attributes

        public event PropertyChangedEventHandler PropertyChanged;
        [XmlIgnore]
        public int ID { get; private set; } = NextId();

        [XmlIgnore]
        private string _name = "";
        [XmlElement(ElementName = "Name")]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        [XmlIgnore]
        private string _desc = "";
        [XmlElement(ElementName = "Description")]
        public string Description
        {
            get { return _desc; }
            set
            {
                _desc = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Description"));
            }
        }

        [XmlIgnore]
        private string _script = "";
        [XmlElement(ElementName = "Script")]
        public string Script
        {
            get { return _script; }
            set
            {
                _script = value.Trim();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Script"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ScriptPreview"));
            }
        }

        [XmlIgnore]
        public string ScriptPreview
        {
            get
            {
                string preview = "";
                var lines = _script.Split(new char[] { '\n' });
                if (lines.Length > 3)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        preview += lines[i] + "\n";
                    }
                    preview += "...";
                }
                else
                {
                    preview = _script;
                }
                return preview;
            }
        }

        [XmlIgnore]
        private bool _showOutput = false;
        [XmlElement(ElementName = "ShowOutput")]
        public bool ShowOutput
        {
            get { return _showOutput; }
            set
            {
                _showOutput = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowOutput"));
            }
        }

        private static int id = 0;

        // Methodes
        
        public override string ToString()
        {
            return string.Format("CommandBlock : {{\n\tName : \"{0}\"\n\tDescription : \"{1}\"\n\tCommand : \"{2}\"\n}}", _name, _desc, _script);
        }

        public bool Equals(Command c)
        {
            return ID.Equals(c.ID) && Name.Equals(c.Name) && Description.Equals(c.Description) && Script.Equals(c.Script) && ShowOutput.Equals(c.ShowOutput);
        }

        public static int NextId()
        {
            return id++;
        }

        public static Command CreateCopy(Command cmd)
        {
            Command c = new Command();
            c.ID = cmd.ID;
            c._name = cmd.Name;
            c._desc = cmd.Description;
            c._script = cmd.Script;
            c._showOutput = cmd.ShowOutput;
            return c;
        }
    }
}
