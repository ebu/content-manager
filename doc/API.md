# EBU Content Manager - API

This document describes the HTTP API availble in the content manager. The API allows dynamical update of variables and valued
using standard HTTP GET and POST.

## API and JSON Data Input over HTTP

### GET Request Data Updates

The HTTP Port allows external processes to update data that is used inside the slides.

#### `/update` Updating Data Variables

The following endpoint allows you to update global variables accross all templates used inside the content manager.

    <server>:<port>/update?variable1=value&variable2=value...

For example if your templates use `@@artist@@`, `@@song@@` and `@@coverurl@@` doing a GET request on

    http://localhost:8080/update?artist=Michael%20Jackson&song=Love%20Never%20Felt%20So%20Good&coverurl=http://coverartarchive.org/release/dfba99ef-8742-425d-a4be-93374d89324c/7148338906-250.jpg

Or to update a show slide with a showname and host

    http://localhost:8080/update?timeslot=09h00%20-%2012h00&showname=Good%20Morning%20Geneva&hostedby=Hosted%20by%20George%20White&gr=sdf

Will update the artist, song and coverurl variables and regenerate all slides that use those variables.

#### IceCast compatible Update

If your station supports updating IceCast endpoints, you can also update the content manager to regenerate slides that contain the
`song` tag. In this case your third party software will call the following url:

    http://localhost:8080/admin/metadata?song=Michael%20Jackson%20-%20Love%20Never%20Felt%20So%20Good

#### Force the regeneration of slides

#### Broadcast a specific slide

#### Change the active cart

Which updates globally the `song` variable used like `@@song@@` in your templates.

### JSON POSTed Data

The HTTP interface allows the content Manager to receive data in JSON format.
The JSON data has to follow the following data structure and have a `datatype`, `key`, `value` and `data` property like the
following example:

```
{
  "datatype": "SONG",
  "key": "123",
  "value": "Song / Artist",
  "data": [
    {
      "datatype": "STRING",
      "key": "ARTIST",
      "value": "Michael Jackson"
    },
    {
      "datatype": "STRING",
      "key": "TITLE",
      "value": "Love Never Felt So Good"
    },
    {
      "datatype": "STRING",
      "key": "ALBUM",
      "value": "XSCAPE"
    }
  ]
}
```
