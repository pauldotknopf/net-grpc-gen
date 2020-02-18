import QtQuick 2.12
import QtQuick.Controls 2.5
import interop 1.0

ApplicationWindow {
    id: window
    visible: true
    width: 640
    height: 480
    title: qsTr("Stack")

    Test1 {
        id: test1

        Component.onCompleted: {

        }
    }
}
