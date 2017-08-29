This class supply a method to visit XML file, XML string, XMLDocment..
It's sample use like JQuery.
You just need add a line 'XmlOperV2.LoadHandle x = ((o) =&gt; { return XmlOperV2.load(o); });' in your own class.
and now you can use it.

eg:<br>
var xmlfile1 = x("filepath....");<br>
xmlfile1.Find("//NodeName").Text("abc");    //change Node Text<br>

xmlfile1.Find("//NodeName").Xml("abc");     //change Node Xml<br>

var result1 = xmlfile1.Find("//NodeName").Text(); //get Node Text<br>

var result2 = xmlfile1.Find("//NodeName").Xml();  //get Node Xml<br>

xmlfile1.Find("//NodeName").CDATA("&lt;test content&gt;&lt;&gt;");  //set Node CDATA Value<br>
  
  var rootNode = x("root");   //Make a &lt;root&gt;&lt;/root&gt; Node<br>
  
  x("root/item");             //Make a tree node &lt;root&gt;&lt;item /&gt;&lt;/root&gt;<br>
  
  x("root").Append(x("item")); //Make a tree node &lt;root&gt;&lt;item /&gt;&lt;/root&gt;<br>
  
  x("root").Append(x("item").Text("TestValue")); //Make a tree node &lt;root&gt;&lt;item&gt;TestValue&lt;/item&gt;&lt;/root&gt;<br>
  
  x("&lt;root xmlns:a=\"test.com\"&gt;&lt;a:item&gt;testValue&lt;/a:item&gt;&lt;/root&gt;").Find("//a:item").Text();  //get Namespace Node Text.<br>
  
  x("root").Append(x("item").Text("张静")).Find("//item").Replace(x("Item").Text("张静"));  //Replace Node.<br>
  
  
Some advanced method:<br>
x("&lt;root&gt;&lt;item&gt;张静&lt;/item&gt;&lt;item&gt;张桢炫&lt;/item&gt;&lt;/root&gt;").Find("//item").Each(delegate(o) {<br>
  Console.WriteLine(o.Text());<br>
  return true;<br>
});<br>

x("&lt;root&gt;&lt;item&gt;张青&lt;/item&gt;&lt;item&gt;张桢炫&lt;/item&gt;&lt;/root&gt;").Find("//item").Eq(0).Text("张静");<br>

All method:&lt;br&gt;
xmlOperObject.Each();   //loop xmlOperObject[];&lt;br&gt;
xmlOperObject.Text();  //get text;&lt;br&gt;
xmlOperObject.Text("abc"); //set text;&lt;br&gt;
xmlOperObject.Xml();   //get innerXML;&lt;br&gt;
xmlOperObject.Xml("&lt;a&gt;abc&lt;/a&gt;");   //set innerXML;&lt;br&gt;
xmlOperObject.CDATA("&lt;a&gt;abc&lt;/a&gt;");  //set as CDATA; like &lt;root&gt;&lt;![CDATA[&lt;a&gt;abc&lt;/a&gt;]]&gt;&gt;&lt;/root&gt;&lt;br&gt;
xmlOperObject.inerertBefore(existsXmlOperObject);&lt;br&gt;
xmlOperObject.InsertAfter(existsXmlOperObject);&lt;br&gt;
xmlOperObject.Append(newXmlOperObject);&lt;br&gt;
xmlOperObject.AppendTo(newXmlOperObject);&lt;br&gt;
xmlOperObject.Replace(anotherXmlOperObject);  &lt;br&gt;
xmlOperObject.Clone();&lt;br&gt;
xmlOperObject.Remove();&lt;br&gt;
xmlOperObject.Childrens();&lt;br&gt;
xmlOperObject.RemoveAttr("attrName");&lt;br&gt;
xmlOperObject.Attr("attrName");  //get attr;&lt;br&gt;
xmlOperObject.Attr("attrName", value);  //set attr;&lt;br&gt;
xmlOperObject.Parent();&lt;br&gt;
xmlOperObject.First();&lt;br&gt;
xmlOperObject.Last();&lt;br&gt;
xmlOperObject.Skip(3);&lt;br&gt;
xmlOperObject.Between(2,5);&lt;br&gt;
xmlOperObject.Top(3);&lt;br&gt;
xmlOperObject.Save(filepath); //Save outerXML to file;


