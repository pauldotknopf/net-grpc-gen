#ifndef GENPROTO_TEST2_WORKER_H
#define GENPROTO_TEST2_WORKER_H
#include <QObject>
#include <QScopedPointer>
#include <QJsonValue>
namespace Tests {
class QTest2WorkerPrivate;
class QTest2Worker : public QObject {
	Q_OBJECT
public:
	QTest2Worker();
	~QTest2Worker();
	void testMethod2(QJsonValue request, int requestId);
	void testMethodSync2(QJsonValue request, int requestId);
	void testMethodWithNoResponse2(QJsonValue request, int requestId);
	void testMethodNoRequest2(int requestId);
signals:
	void testMethod2Done(QJsonValue val, int requestId, QString error);
	void testMethodSync2Done(QJsonValue val, int requestId, QString error);
	void testMethodWithNoResponse2Done(int requestId, QString error);
	void testMethodNoRequest2Done(int requestId, QString error);
private:
	QScopedPointer<QTest2WorkerPrivate> const d_priv;
};
}
#endif // GENPROTO_TEST2_WORKER_H
