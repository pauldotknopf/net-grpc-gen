#ifndef QROCCOMMON_H
#define QROCCOMMON_H

#include <QJsonValue>
#include <QJSValue>

class QJSEngine;
QJSValue convertJsonValueToJsValue(QJSEngine* engine, QJsonValue& val);

#endif // QROCCOMMON_H
