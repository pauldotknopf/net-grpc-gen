import QtQuick 2.12
import QtQuick.Controls 2.5
import interop 1.0

ApplicationWindow {
    id: window
    visible: true
    width: 640
    height: 480
    title: qsTr("Stack")

    property int requestNumber: 0;

    Timer {
        running: true
        repeat: true
        interval: 10
        onTriggered: {
            gc()
        }
    }

    Timer {
        running: true
        repeat: true
        interval: 1
        onTriggered: {
            requestNumber++;
            test1.testMethodNoRequest("state " + requestNumber, function(e) {
                console.log("testMethodNoRequest callback!")
                console.log(JSON.stringify(e))
            })
            test1.testMethodPrimitive(requestNumber, "state " + requestNumber, function(e) {
                console.log("testMethodPrimitive callback!")
                console.log(JSON.stringify(e))
            });
        }
    }

    Test1 {
        id: test1

        Component.onCompleted: {
        }
    }
}
