#ifndef GENPROTO_TEST2_WORKER_H
#define GENPROTO_TEST2_WORKER_H
#include <QObject>
#include <QScopedPointer>
namespace Tests {
class QTest2WorkerPrivate;
class QTest2Worker : public QObject {
	Q_OBJECT
public:
	QTest2Worker();
	~QTest2Worker();
signals:
private:
	QScopedPointer<QTest2WorkerPrivate> const d_priv;
};
}
#endif // GENPROTO_TEST2_WORKER_H
