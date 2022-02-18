using Xunit;
using TestAttribute = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Linq;

[TestFixture]
public class AnnotationsTests : TestFixtureBase
{
    [Fact]
    public void AddAnnotation()
    {
        var o = new JObject();
        o.AddAnnotation("A string!");

        var s = o.Annotation<string>();
        Assert.AreEqual("A string!", s);

        s = (string)o.Annotation(typeof(string));
        Assert.AreEqual("A string!", s);
    }

    [Fact]
    public void AddAnnotation_MultipleOfTheSameType()
    {
        var o = new JObject();
        o.AddAnnotation("A string!");
        o.AddAnnotation("Another string!");

        var s = o.Annotation<string>();
        Assert.AreEqual("A string!", s);

        s = (string)o.Annotation(typeof(string));
        Assert.AreEqual("A string!", s);
    }

    [Fact]
    public void AddAnnotation_MultipleOfDifferentTypes()
    {
        var o = new JObject();
        o.AddAnnotation("A string!");
        o.AddAnnotation(new Uri("http://www.google.com/"));

        var s = o.Annotation<string>();
        Assert.AreEqual("A string!", s);

        s = (string)o.Annotation(typeof(string));
        Assert.AreEqual("A string!", s);

        var i = o.Annotation<Uri>();
        Assert.AreEqual(new Uri("http://www.google.com/"), i);

        i = (Uri)o.Annotation(typeof(Uri));
        Assert.AreEqual(new Uri("http://www.google.com/"), i);
    }

    [Fact]
    public void GetAnnotation_NeverSet()
    {
        var o = new JObject();

        var s = o.Annotation<string>();
        Assert.AreEqual(null, s);

        s = (string)o.Annotation(typeof(string));
        Assert.AreEqual(null, s);
    }

    [Fact]
    public void GetAnnotations()
    {
        var o = new JObject();
        o.AddAnnotation("A string!");
        o.AddAnnotation("A string 2!");
        o.AddAnnotation("A string 3!");

        IList<string> l = o.Annotations<string>().ToList();

        Assert.AreEqual(3, l.Count);
        Assert.AreEqual("A string!", l[0]);
        Assert.AreEqual("A string 2!", l[1]);
        Assert.AreEqual("A string 3!", l[2]);

        l = o.Annotations(typeof(string)).Cast<string>().ToList();

        Assert.AreEqual(3, l.Count);
        Assert.AreEqual("A string!", l[0]);
        Assert.AreEqual("A string 2!", l[1]);
        Assert.AreEqual("A string 3!", l[2]);
    }

    [Fact]
    public void GetAnnotations_MultipleTypes()
    {
        var o = new JObject();
        o.AddAnnotation("A string!");
        o.AddAnnotation("A string 2!");
        o.AddAnnotation("A string 3!");
        o.AddAnnotation(new Uri("http://www.google.com/"));

        IList<object> l = o.Annotations<object>().ToList();

        Assert.AreEqual(4, l.Count);
        Assert.AreEqual("A string!", l[0]);
        Assert.AreEqual("A string 2!", l[1]);
        Assert.AreEqual("A string 3!", l[2]);
        Assert.AreEqual(new Uri("http://www.google.com/"), l[3]);

        l = o.Annotations(typeof(object)).ToList();

        Assert.AreEqual(4, l.Count);
        Assert.AreEqual("A string!", l[0]);
        Assert.AreEqual("A string 2!", l[1]);
        Assert.AreEqual("A string 3!", l[2]);
        Assert.AreEqual(new Uri("http://www.google.com/"), l[3]);
    }

    [Fact]
    public void RemoveAnnotation()
    {
        var o = new JObject();
        o.AddAnnotation("A string!");

        o.RemoveAnnotations<string>();

        var s = o.Annotation<string>();
        Assert.AreEqual(null, s);
    }

    [Fact]
    public void RemoveAnnotation_NonGeneric()
    {
        var o = new JObject();
        o.AddAnnotation("A string!");

        o.RemoveAnnotations(typeof(string));

        var s = o.Annotation<string>();
        Assert.AreEqual(null, s);

        s = (string)o.Annotation(typeof(string));
        Assert.AreEqual(null, s);
    }

    [Fact]
    public void RemoveAnnotation_Multiple()
    {
        var o = new JObject();
        o.AddAnnotation("A string!");
        o.AddAnnotation("A string 2!");
        o.AddAnnotation("A string 3!");

        o.RemoveAnnotations<string>();

        var s = o.Annotation<string>();
        Assert.AreEqual(null, s);

        o.AddAnnotation("A string 4!");

        s = o.Annotation<string>();
        Assert.AreEqual("A string 4!", s);

        var i = (Uri)o.Annotation(typeof(Uri));
        Assert.AreEqual(null, i);
    }

    [Fact]
    public void RemoveAnnotation_MultipleCalls()
    {
        var o = new JObject();
        o.AddAnnotation("A string!");
        o.AddAnnotation(new Uri("http://www.google.com/"));

        o.RemoveAnnotations<string>();
        o.RemoveAnnotations<Uri>();

        var s = o.Annotation<string>();
        Assert.AreEqual(null, s);

        var i = o.Annotation<Uri>();
        Assert.AreEqual(null, i);
    }

    [Fact]
    public void RemoveAnnotation_Multiple_NonGeneric()
    {
        var o = new JObject();
        o.AddAnnotation("A string!");
        o.AddAnnotation("A string 2!");

        o.RemoveAnnotations(typeof(string));

        var s = o.Annotation<string>();
        Assert.AreEqual(null, s);
    }

    [Fact]
    public void RemoveAnnotation_MultipleCalls_NonGeneric()
    {
        var o = new JObject();
        o.AddAnnotation("A string!");
        o.AddAnnotation(new Uri("http://www.google.com/"));

        o.RemoveAnnotations(typeof(string));
        o.RemoveAnnotations(typeof(Uri));

        var s = o.Annotation<string>();
        Assert.AreEqual(null, s);

        var i = o.Annotation<Uri>();
        Assert.AreEqual(null, i);
    }

    [Fact]
    public void RemoveAnnotation_MultipleWithDifferentTypes()
    {
        var o = new JObject();
        o.AddAnnotation("A string!");
        o.AddAnnotation(new Uri("http://www.google.com/"));

        o.RemoveAnnotations<string>();

        var s = o.Annotation<string>();
        Assert.AreEqual(null, s);

        var i = o.Annotation<Uri>();
        Assert.AreEqual(new Uri("http://www.google.com/"), i);
    }

    [Fact]
    public void RemoveAnnotation_MultipleWithDifferentTypes_NonGeneric()
    {
        var o = new JObject();
        o.AddAnnotation("A string!");
        o.AddAnnotation(new Uri("http://www.google.com/"));

        o.RemoveAnnotations(typeof(string));

        var s = o.Annotation<string>();
        Assert.AreEqual(null, s);

        var i = o.Annotation<Uri>();
        Assert.AreEqual(new Uri("http://www.google.com/"), i);
    }

    [Fact]
    public void AnnotationsAreCopied()
    {
        var o = new JObject();
        o.AddAnnotation("string!");
        AssertCloneCopy(o, "string!");

        var p = new JProperty("Name", "Content");
        p.AddAnnotation("string!");
        AssertCloneCopy(p, "string!");

        var a = new JArray();
        a.AddAnnotation("string!");
        AssertCloneCopy(a, "string!");

        var c = new JConstructor("Test");
        c.AddAnnotation("string!");
        AssertCloneCopy(c, "string!");

        var v = new JValue(true);
        v.AddAnnotation("string!");
        AssertCloneCopy(v, "string!");

        var r = new JRaw("raw");
        r.AddAnnotation("string!");
        AssertCloneCopy(r, "string!");
    }

    [Fact]
    public void MultipleAnnotationsAreCopied()
    {
        var version = new Version(1, 2, 3, 4);

        var o = new JObject();
        o.AddAnnotation("string!");
        o.AddAnnotation(version);

        var o2 = (JObject)o.DeepClone();
        Assert.AreEqual("string!", o2.Annotation<string>());
        Assert.AreEqual(version, o2.Annotation<Version>());

        o2.RemoveAnnotations<Version>();
        Assert.AreEqual(1, o.Annotations<Version>().Count());
        Assert.AreEqual(0, o2.Annotations<Version>().Count());
    }

    void AssertCloneCopy<T>(JToken t, T annotation) where T : class
    {
        Assert.AreEqual(annotation, t.DeepClone().Annotation<T>());
    }

    [Fact]
    public void Example()
    {
        var o = JObject.Parse(@"{
                'name': 'Bill G',
                'age': 58,
                'country': 'United States',
                'employer': 'Microsoft'
            }");

        o.AddAnnotation(new HashSet<string>());
        o.PropertyChanged += (_, args) => o.Annotation<HashSet<string>>().Add(args.PropertyName);

        o["age"] = 59;
        o["employer"] = "Bill & Melinda Gates Foundation";

        var changedProperties = o.Annotation<HashSet<string>>();
        // age
        // employer

        Assert.AreEqual(2, changedProperties.Count);
    }
}