#ifndef QTEST1_H
#define QTEST1_H

#include <QObject>

#include <QScopedPointer>
class QTest1Private;

class QTest1 : public QObject
{
    Q_OBJECT
    Q_PROPERTY(QString propString READ getPropString WRITE setPropString NOTIFY propStringChanged)
public:
    QTest1();
    ~QTest1();

    QString getPropString();
    void setPropString(QString& val);

signals:
    void propStringChanged(QString val);

private:
    QScopedPointer<QTest1Private> const d_ptr;
};

#endif // QTEST1_H
