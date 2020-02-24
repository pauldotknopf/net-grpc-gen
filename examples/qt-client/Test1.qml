import QtQuick 2.0
import interop 1.0

Item {
    Component.onCompleted: {
    }

    Test1 {
        id: test
        onTestEvent: function(value) {
            console.log(value)
        }
        onTestEventNoData: {
            console.log("no data!")
        }
    }
}
