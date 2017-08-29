This class supply a method to visit XML file, XML string, XMLDocment..
It's sample use like JQuery.
You just need add a line 'XmlOperV2.LoadHandle x = ((o) => { return XmlOperV2.load(o); });' in your own class.
and now you can use it.

eg:<br /> 
var xmlfile1 = x("filepath....");
xmlfile1.Find("//NodeName").Text("abc");    //change Node Text

xmlfile1.Find("//NodeName").Xml("abc");     //change Node Xml

var result1 = xmlfile1.Find("//NodeName").Text(); //get Node Text

var result2 = xmlfile1.Find("//NodeName").Xml();  //get Node Xml

xmlfile1.Find("//NodeName").CDATA("<test content><>");  //set Node CDATA Value
  
  var rootNode = x("root");   //Make a <root></root> Node
  
  x("root/item");             //Make a tree node <root><item /></root>
  
  x("root").Append(x("item")); //Make a tree node <root><item /></root>
  
  x("root").Append(x("item").Text("TestValue")); //Make a tree node <root><item>TestValue</item></root>
  
  x("<root xmlns:a=\"test.com\"><a:item>testValue</a:item></root>").Find("//a:item").Text();  //get Namespace Node Text.
  
  x("root").Append(x("item").Text("张静")).Find("//item").Replace(x("Item").Text("张静"));  //Replace Node.
  
  
