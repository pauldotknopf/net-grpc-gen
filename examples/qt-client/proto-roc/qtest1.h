#ifndef GENPROTO_TEST1_H
#define GENPROTO_TEST1_H
#include <QObject>
#include <QScopedPointer>
#include <QJSValue>
namespace Tests {
class QTest1Private;
class QTest1 : public QObject {
	Q_OBJECT
public:
	QTest1(QObject* parent = nullptr);
	~QTest1();
	Q_INVOKABLE void testMethodNoRequest(QJSValue state, QJSValue callback);
private slots:
	void testMethodNoRequestHandler(int requestId, QString error);
private:
	QScopedPointer<QTest1Private> const d_priv;
};
}
#endif // GENPROTO_TEST1_H
