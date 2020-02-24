#ifndef GENPROTO_TEST2_H
#define GENPROTO_TEST2_H
#include <QObject>
#include <QScopedPointer>
#include <QJSValue>
#include <QJsonValue>
namespace Tests {
class QTest2Private;
class QTest2 : public QObject {
	Q_OBJECT
	Q_PROPERTY(QJsonValue propString2 READ getPropString2 WRITE setPropString2 NOTIFY propString2Changed)
	Q_PROPERTY(QJsonValue propComplex2 READ getPropComplex2 WRITE setPropComplex2 NOTIFY propComplex2Changed)
public:
	QTest2(QObject* parent = nullptr);
	~QTest2();
	Q_INVOKABLE void testMethod2(QJsonValue val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testMethodSync2(QJsonValue val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testMethodWithNoResponse2(QJsonValue val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testMethodNoRequest2(QJSValue state, QJSValue callback);
	QJsonValue getPropString2();
	void setPropString2(QJsonValue val);
	QJsonValue getPropComplex2();
	void setPropComplex2(QJsonValue val);
signals:
	void testEvent2(QJsonValue val);
	void testEventComplex2(QJsonValue val);
	void testEventNoData2();
	void propString2Changed(QJsonValue val);
	void propComplex2Changed(QJsonValue val);
private slots:
	void testMethod2Handler(QJsonValue result, int requestId, QString error);
	void testMethodSync2Handler(QJsonValue result, int requestId, QString error);
	void testMethodWithNoResponse2Handler(int requestId, QString error);
	void testMethodNoRequest2Handler(int requestId, QString error);
private:
	QScopedPointer<QTest2Private> const d_priv;
};
}
#endif // GENPROTO_TEST2_H
