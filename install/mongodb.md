# MongoDB Installation
A MongoDB server is needed, if you want to use the MongoDbBlackBox. Either download MongoDB from the [MongoDB homepage](http://www.mongodb.org/downloads), install it locally, and run it - or use a MongoDB instance in the cloud, e.g. at [MongoHQ](http://www.mongohq.com/home).

Then instantiate MongoDbBlackBox with an appropriate connection string, e.g.

```
var bb = new MongoDbBlackBox("mongodb://localhost:27017");
...
```