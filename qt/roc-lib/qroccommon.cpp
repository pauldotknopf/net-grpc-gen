#include "roc-lib/qroccommon.h"
#include <QJsonObject>
#include <QJSEngine>
#include <QJsonArray>

QJSValue convertJsonValueToJsValue(QJSEngine* engine, QJsonValue& val)
{
    if (val.isBool())
    {
        return QJSValue(val.toBool());
    }
    else if (val.isString())
    {
        return QJSValue(val.toString());
    }
    else if (val.isDouble())
    {
        return QJSValue(val.toDouble());
    }
    else if (val.isNull())
    {
        return QJSValue(QJSValue::NullValue);
    }
    else if (val.isUndefined())
    {
        return QJSValue(QJSValue::UndefinedValue);
    }
    else if (val.isObject())
    {
        QJsonObject obj = val.toObject();
        QJSValue newobj = engine->newObject();
        for (auto itor = obj.begin(); itor != obj.end(); itor++)
        {
            QString key = itor.key();
            QJsonValue value = itor.value();
            QJSValue convertedValue = convertJsonValueToJsValue(engine, value);
            newobj.setProperty(key, convertedValue);
        }
        return newobj;
    }
    else if (val.isArray())
    {
        QJsonArray arr = val.toArray();
        QJSValue newobj = engine->newArray(arr.size());
        for (int i = 0; i < arr.size(); i++)
        {
            QJsonValue value = arr[i];
            QJSValue convertedValue = convertJsonValueToJsValue(engine, value);
            newobj.setProperty(i, convertedValue);
        }
        return newobj;
    }


    // ASSERT(FALSE && "This shouldn't happen");
    return QJSValue(QJSValue::UndefinedValue);
}
