#ifndef GENPROTO_TEST1_WORKER_H
#define GENPROTO_TEST1_WORKER_H
#include <QObject>
#include <QScopedPointer>
#include <QJsonValue>
#include <QVariant>
namespace Tests {
class QTest1WorkerPrivate;
class QTest1Worker : public QObject {
	Q_OBJECT
public:
	QTest1Worker();
	~QTest1Worker();
	void testMethod(QJsonValue request, int requestId);
	void testMethodSync(QJsonValue request, int requestId);
	void testMethodWithNoResponse(QJsonValue request, int requestId);
	void testMethodPrimitive(bool request, int requestId);
	void testMethodNoRequestOrResponse(int requestId);
	void testMethodNoRequest(int requestId);
	void testMethodNoResponse(bool request, int requestId);
	void processEvent(void* event);
	QVariant getPropString();
	void setPropString(QVariant val);
	QJsonValue getPropComplex();
	void setPropComplex(QJsonValue val);
signals:
	void testMethodDone(QJsonValue val, int requestId, QString error);
	void testMethodSyncDone(QJsonValue val, int requestId, QString error);
	void testMethodWithNoResponseDone(int requestId, QString error);
	void testMethodPrimitiveDone(bool val, int requestId, QString error);
	void testMethodNoRequestOrResponseDone(int requestId, QString error);
	void testMethodNoRequestDone(bool val, int requestId, QString error);
	void testMethodNoResponseDone(int requestId, QString error);
	void testEventRaised(QVariant val);
	void testEventComplexRaised(QJsonValue val);
	void testEventNoDataRaised();
	void propStringChanged(QVariant val);
	void propComplexChanged(QJsonValue val);
private:
	QScopedPointer<QTest1WorkerPrivate> const d_priv;
};
}
#endif // GENPROTO_TEST1_WORKER_H
