#ifndef GENPROTO_TEST1_WORKER_H
#define GENPROTO_TEST1_WORKER_H
#include <QObject>
#include <QScopedPointer>
#include <QJsonValue>
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
	void testMethodPrimitive(int request, int requestId);
	void testMethodNoRequestOrResponse(int requestId);
	void testMethodNoRequest(int requestId);
	void testMethodNoResponse(int request, int requestId);
signals:
	void testMethodDone(QJsonValue val, int requestId, QString error);
	void testMethodSyncDone(QJsonValue val, int requestId, QString error);
	void testMethodWithNoResponseDone(int requestId, QString error);
	void testMethodPrimitiveDone(int val, int requestId, QString error);
	void testMethodNoRequestOrResponseDone(int requestId, QString error);
	void testMethodNoRequestDone(int val, int requestId, QString error);
	void testMethodNoResponseDone(int requestId, QString error);
private:
	QScopedPointer<QTest1WorkerPrivate> const d_priv;
};
}
#endif // GENPROTO_TEST1_WORKER_H
