#ifndef GENPROTO_TEST2_H
#define GENPROTO_TEST2_H
#include <QObject>
#include <QScopedPointer>
#include <QJSValue>
namespace Tests {
class QTest2Private;
class QTest2 : public QObject {
	Q_OBJECT
public:
	QTest2(QObject* parent = nullptr);
	~QTest2();
private slots:
private:
	QScopedPointer<QTest2Private> const d_priv;
};
}
#endif // GENPROTO_TEST2_H
