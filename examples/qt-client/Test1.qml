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
            if(testInt == 1) {
                test.propComplex = null;
            } else if(testInt == 2) {
                test.propComplex = {value1: 1}
                testInt = 0
            }
        }
    }

    Test1 {
        id: test
        onTestEvent: function(value) {
            console.log(value)
        }
        onTestEventNoData: {
            console.log("no data!")
        }
        onPropComplexChanged: {
            console.log("prop complex changed...")
            console.log(typeof test.propComplex)
            if(test.propComplex) {
                console.log(JSON.stringify(test.propComplex))
            }
        }
        onPropStringChanged: {
            console.log("prop string changed: " + test.propString)
            console.log(typeof test.propString)
        }
    }

    Text {
        text: test.propString ? test.propString : ""
    }
}
