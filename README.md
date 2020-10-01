# AppStarter

A Windows tool to create a simple AppStarter constructed by a xml data file.

No Installation needed, just download the Release and run it.
It just created the following XML file for defining the shortcuts.

ATTENTION:
The apps detects the AppStartItem by Text (Name), so do not use the same name twice. ;)

You are still free to fork and customize it for you needs.

NJOY.

# AppStarter.data

This is the main file containing the shortcuts.
The XML looks like this:

    <?xml version="1.0"?>
    <AppStartData xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
      <Items>
        <AppStartItem>
          <Text>Explorer Run</Text>
          <Category>Tools</Category>
          <Path>C:\WINDOWS\explorer.exe</Path>
          <Arguments>shell:::{2559a1f3-21d7-11d4-bdaf-00c04f60b9f0}</Arguments>
        </AppStartItem>
      </Items>
    </AppStartData>

Each AppStartItem should be placed within <Items> Tag.

Add new item in the XML file as needed.
