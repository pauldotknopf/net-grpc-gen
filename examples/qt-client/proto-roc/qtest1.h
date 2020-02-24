#ifndef GENPROTO_TEST1_H
#define GENPROTO_TEST1_H
#include <QObject>
#include <QScopedPointer>
#include <QJSValue>
#include <QJsonValue>
namespace Tests {
class QTest1Private;
class QTest1 : public QObject {
	Q_OBJECT
	Q_PROPERTY(QJsonValue propString READ getPropString WRITE setPropString NOTIFY propStringChanged)
	Q_PROPERTY(QJsonValue propComplex READ getPropComplex WRITE setPropComplex NOTIFY propComplexChanged)
public:
	QTest1(QObject* parent = nullptr);
	~QTest1();
	Q_INVOKABLE void testMethod(QJsonValue val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testMethodSync(QJsonValue val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testMethodWithNoResponse(QJsonValue val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testMethodPrimitive(bool val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testMethodNoRequestOrResponse(QJSValue state, QJSValue callback);
	Q_INVOKABLE void testMethodNoRequest(QJSValue state, QJSValue callback);
	Q_INVOKABLE void testMethodNoResponse(bool val, QJSValue state, QJSValue callback);
	QJsonValue getPropString();
	void setPropString(QJsonValue val);
	QJsonValue getPropComplex();
	void setPropComplex(QJsonValue val);
signals:
	void testEvent(QJsonValue val);
	void testEventComplex(QJsonValue val);
	void testEventNoData();
	void propStringChanged(QJsonValue val);
	void propComplexChanged(QJsonValue val);
private slots:
	void testMethodHandler(QJsonValue result, int requestId, QString error);
	void testMethodSyncHandler(QJsonValue result, int requestId, QString error);
	void testMethodWithNoResponseHandler(int requestId, QString error);
	void testMethodPrimitiveHandler(bool result, int requestId, QString error);
	void testMethodNoRequestOrResponseHandler(int requestId, QString error);
	void testMethodNoRequestHandler(bool result, int requestId, QString error);
	void testMethodNoResponseHandler(int requestId, QString error);
private:
	QScopedPointer<QTest1Private> const d_priv;
};
}
#endif // GENPROTO_TEST1_H
