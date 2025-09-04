//Declare Global Variable for Collection....
//var AttributeListObject = new Object();

//function Name...
function clsXmlUtility() {

    this.AttributeListObject = new Object();

    this.XML = [];
    this.Nodes = [];
    this.State = "";
    this.FormatXML = function (Str, blFoundCondition) {

        if (Str.length > 0) {
            Str = Str.toString();
            Str = Str.replace(/&/g, "&amp;").replace(/\"/g, "&quot;");
            if (!blFoundCondition) {
                Str = Str.replace(/</g, "&lt;").replace(/>/g, "&gt;");
            }
            return Str;
        }
        return "";
    }
    this.BeginNode = function (Name) {
        if (!Name) return;
        if (this.State == "beg") this.XML.push(">");
        this.State = "beg";
        this.Nodes.push(Name);
        this.XML.push("<" + Name);
    }
    this.EndNode = function () {
        if (this.State == "beg") {
            this.XML.push("/>");
            this.Nodes.pop();
        }
        else if (this.Nodes.length > 0)
            this.XML.push("</" + this.Nodes.pop() + ">");
        this.State = "";
    }
    this.Attrib = function (Name, Value) {
        if (this.State != "beg" || !Name) return;
        this.XML.push(" " + Name + "=\"" + this.FormatXML(Value) + "\"");
    }
    this.WriteString = function (Value) {
        if (this.State == "beg") this.XML.push(">");
        this.XML.push(this.FormatXML(Value));
        this.State = "";
    }
    this.Node = function (Name, Value, blFoundCondition) {
        if (!Name) return;
        if (this.State == "beg") this.XML.push(">");
        this.XML.push((Value == "" || !Value) ? "<" + Name + "/>" : "<" + Name + ">" + this.FormatXML(Value, blFoundCondition) + "</" + Name + ">");
        this.State = "";
    }
    this.Close = function () {
        while (this.Nodes.length > 0)
            this.EndNode();
        this.State = "closed";
    }
    this.ToString = function () { return this.XML.join(""); }

    //this function is used to generate key - value pair for xml atribute object...
    this.AddToList = function (key, val) {
        this.AttributeListObject[key] = val;
    };

    this.GenerateValidXML = function (elementName, innerContent, AttributelistObject, closingtag) {

        if (this.XML.length == 0) {
            this.BeginNode("Sql");
        }

        //Return if starting tag is empty
        if (!elementName) {
            this.AttributeListObject = new Object();
            return;
        }

        var strCondition = "";
        if (elementName == "where") {
            for (var i in AttributelistObject) {
                strCondition = "<![CDATA[ " + i + "='" + AttributelistObject[i] + "']]>";
            }
            this.Node(elementName, strCondition, true);
            this.AttributeListObject = new Object();
            return;
        }

        this.BeginNode(elementName);

        if (typeof AttributelistObject != "undefined") {
            for (var i in AttributelistObject) {
                if (i === "Table") {
                    this.Attrib(i, AttributelistObject[i]);
                } else if (i === "GenerateID") {
                    if (AttributelistObject[i]) {
                        this.Attrib(i, "Yes");
                    } else {
                        this.Attrib(i, "No");
                    }
                } else {
                    this.Attrib(i, AttributelistObject[i]);
                }
            }
        }

        if (!innerContent) {
        } else {
            this.WriteString(innerContent);
        }

        if (typeof closingtag == "undefined") {
            this.EndNode();
        }

        this.AttributeListObject = new Object();
    };
};



