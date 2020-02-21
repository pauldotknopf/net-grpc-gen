#ifndef QROCCOMMON_H
#define QROCCOMMON_H

#include <QJSValue>
#include <QSharedPointer>

struct QJSValueContainer
{
    QJSValue value;
};

Q_DECLARE_METATYPE(QJSValueContainer)
Q_DECLARE_METATYPE(QSharedPointer<QJSValueContainer>)

#endif // QROCCOMMON_H
