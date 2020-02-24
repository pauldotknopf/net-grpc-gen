import QtQuick 2.12
import QtQuick.Controls 2.5

ApplicationWindow {
    id: window
    visible: true
    width: 640
    height: 480

    Timer {
        running: true
        repeat: true
        interval: 5000
        onTriggered: {
            //pageLoader.source = ""
            //pageLoader.source = "Test1.qml"
        }
    }

    Loader {
        id: pageLoader
        source: "Test1.qml"
    }
}
