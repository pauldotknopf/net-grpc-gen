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
public:
	QTest2(QObject* parent = nullptr);
	~QTest2();
	Q_INVOKABLE void testMethod2(QJsonValue val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testMethodSync2(QJsonValue val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testMethodWithNoResponse2(QJsonValue val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testMethodNoRequest2(QJSValue state, QJSValue callback);
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
