import QtQuick 2.0
import interop 1.0

Item {
    property int testInt: 0

    Timer {
        running: true
        repeat: true
        interval: 1000
        onTriggered: {
            testInt++;
            test.TestParamByte()
        }
    }

    TestTypes {
        id: test
        Component.onCompleted: {
        }
    }

    Text {
        text: test.propString ? test.propString : ""
    }
}
