#ifndef GENPROTO_TEST1_WORKER_H
#define GENPROTO_TEST1_WORKER_H
#include <QObject>
#include <QScopedPointer>
namespace Tests {
class QTest1WorkerPrivate;
class QTest1Worker : public QObject {
	Q_OBJECT
public:
	QTest1Worker();
	~QTest1Worker();
	void testMethodNoRequest(int requestId);
signals:
	void testMethodNoRequestDone(int requestId, QString error);
private:
	QScopedPointer<QTest1WorkerPrivate> const d_priv;
};
}
#endif // GENPROTO_TEST1_WORKER_H
