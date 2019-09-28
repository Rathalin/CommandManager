using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommandManager.Data
{
    public class CommandGroup
    {
        // Constructors

        public CommandGroup()
        {
            CommandList = new ObservableCollection<Command>();
        }

        // Attributes and Variables

        [XmlElement(ElementName = "Groupname")]
        public string Name { get; set; }

        [XmlElement(ElementName = "CommandList")]
        public ObservableCollection<Command> CommandList { get; set; }
    }
}
