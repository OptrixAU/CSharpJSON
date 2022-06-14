# CSharpJSON
A very simple API for accessing JSON data from C#, without need to create classes.

Although I'd expect there are far better - and more tested - libraries out there, I've been constantly frustrated with
how most C# JSON libraries concentrate on creating objects, rather than exchanging data.

The **JSONDecoder** class allows you to get data out of JSON by treating it as a set of dictionaries and arrays, rather than a set of objects.

Given the JSON below...

```
{
   "name": "centipede",
   "type": "insect",
   "body": {
       "legs": 60,
	   "length": {
	        "min": 20,
			"max": 60
			}
	,
	"species": [
		"there",
		"are",
		"many"
	]
}
```

You'd first parse the JSON using

```
Decoded = new JSONDecoder(jsondata);
```

Then you can get information using it as an array. For example, `Decoded["body"]["length"]["max"].Text` would get the maximum length of an adult centipede.

